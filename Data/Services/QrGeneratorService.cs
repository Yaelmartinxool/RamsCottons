using QRCoder;
using System.Text;

namespace RamsCottons.Services
{
    public class QrGeneratorService
    {
        public string GenerarQRBase64(string texto, int pixelesPorModulo = 20)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    var qrCodeBytes = qrCode.GetGraphic(pixelesPorModulo);
                    return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
                }
            }
        }
    }
}