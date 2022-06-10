using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using ScreenCommands = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

namespace Distance {
    internal class OledDisplay : Device
    {
        private Ssd1306 _screen;

        public OledDisplay(GpioController controller) : base(controller)
        {
            _screen = new Ssd1306(I2cDevice.Create(new(1, 0x3C)));

            Initialize();
        } 
        
        private void Initialize() 
        {
            _screen.SendCommand(new SetDisplayOff());
            _screen.SendCommand(new ScreenCommands.SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
            _screen.SendCommand(new SetMultiplexRatio(0x1F));
            _screen.SendCommand(new ScreenCommands.SetDisplayOffset(0x00));
            _screen.SendCommand(new ScreenCommands.SetDisplayStartLine(0x00));
            _screen.SendCommand(new ScreenCommands.SetChargePump(true));
            _screen.SendCommand(new ScreenCommands.SetMemoryAddressingMode(ScreenCommands.SetMemoryAddressingMode.AddressingMode.Horizontal));
            _screen.SendCommand(new ScreenCommands.SetSegmentReMap(true));
            _screen.SendCommand(new ScreenCommands.SetComOutputScanDirection(false));
            _screen.SendCommand(new ScreenCommands.SetComPinsHardwareConfiguration(false, false));
            _screen.SendCommand(new SetContrastControlForBank0(0x8F));
            _screen.SendCommand(new ScreenCommands.SetPreChargePeriod(0x01, 0x0F));
            _screen.SendCommand(new ScreenCommands.SetVcomhDeselectLevel(ScreenCommands.SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
            _screen.SendCommand(new ScreenCommands.EntireDisplayOn(false));
            _screen.SendCommand(new ScreenCommands.SetNormalDisplay());
            _screen.SendCommand(new SetDisplayOn());
            _screen.SendCommand(new ScreenCommands.SetColumnAddress());
            _screen.SendCommand(new ScreenCommands.SetPageAddress(ScreenCommands.PageAddress.Page0, ScreenCommands.PageAddress.Page7));
        }

        public void Message(string message)
        {
            Clear();
            _screen.SendCommand(new ScreenCommands.SetColumnAddress(0));
            _screen.SendCommand(new ScreenCommands.SetPageAddress(ScreenCommands.PageAddress.Page0, ScreenCommands.PageAddress.Page7));

            foreach (char character in message)
            {
                _screen.SendData(Font.GetCharacterBytes(character));
            }
        }
        
        public void DisplayClock()
        {
            Clear();
            Console.WriteLine("Display clock");
            var systemFont = SystemFonts.CreateFont("DejaVu Sans", 24, FontStyle.Italic);
            var height = 0;

            foreach (var i in Enumerable.Range(0, 100))
            {
                using (Image<Rgba64> image = new Image<Rgba64>(128, 64))
                {
                    image.Mutate(ctx => ctx
                        .DrawText(DateTime.Now.ToString("HH:mm:ss"), systemFont, Color.White,
                            new SixLabors.ImageSharp.PointF(0, height)));

                    using (Image<L16> image_t = image.CloneAs<L16>())
                    {
                        DisplayImage(image_t);
                    }

                    height++;
                    if (height >= image.Height)
                    {
                        height = 0;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        void DisplayImage(Image<L16> image)
        {
            Int16 width = 128;
            Int16 pages = 8;
            List<byte> buffer = new();

            for (int page = 0; page < pages; page++)
            {
                for (int x = 0; x < width; x++)
                {
                    int bits = 0;
                    for (byte bit = 0; bit < 8; bit++)
                    {
                        bits = bits << 1;
                        bits |= image[x, page * 8 + 7 - bit].PackedValue > 0 ? 1 : 0;
                    }

                    buffer.Add((byte)bits);
                }
            }

            int chunk_size = 16;
            for (int i = 0; i < buffer.Count; i += chunk_size)
            {
                _screen.SendData(buffer.Skip(i).Take(chunk_size).ToArray());
            }
        }
       
        private void Clear()
        {
            _screen.SendCommand(new ScreenCommands.SetColumnAddress());
            _screen.SendCommand(new ScreenCommands.SetPageAddress(ScreenCommands.PageAddress.Page0, ScreenCommands.PageAddress.Page7));

            for (int cnt = 0; cnt < 64; cnt++)
            {
                byte[] data = new byte[16];
                _screen.SendData(data);
            }
        }
    }
}