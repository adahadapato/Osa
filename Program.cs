// See https://aka.ms/new-console-template for more information
using DbfReaderNET;
using System.Text;
using ZXing.Common;
using ZXing;
using System.Drawing;
//using System.CommandLine;


//static void Main(string[] args)
//{
//    Console.WriteLine(ReadDbf(args[0], args[1]).GetAwaiter().GetResult());
//}

Console.WriteLine("Please enter the dbf  file path and file name");
var _fileToRead = Console.ReadLine();

Console.WriteLine("Please enter the qrcode file path");
var _QRCodePath = Console.ReadLine();

//string _fileToRead = Environment.GetCommandLineArgs()[0];
//string _QRCodePath = Environment.GetCommandLineArgs()[1];

Console.WriteLine(ReadDbf(_fileToRead, _QRCodePath).GetAwaiter().GetResult());
Console.ReadLine(); 
static async Task<string> ReadDbf(string fileToRead, string QRCodePath)
{
    return await Task.Run(() =>
    {
        if (string.IsNullOrEmpty(fileToRead))
        {
            return "Invalid dbf path";
        }

        if (string.IsNullOrEmpty(QRCodePath))
        {
            Console.WriteLine("No qrcode directory specified, data will be stored on this directory");
        }

        if (!File.Exists(fileToRead))
        {
            return $"File {fileToRead} does not exist";
        }
        if (!string.IsNullOrWhiteSpace(QRCodePath))
        {
            if (!Directory.Exists(QRCodePath))
            {
                Directory.CreateDirectory(QRCodePath);
            }
        }
       

        Dbf _dbf = new();
        var _mdata = new StringBuilder();
        _dbf.Read(fileToRead);
        foreach (DbfRecord record in _dbf.Records)
        {
            if (record == null) continue;
            var SchoolNo = record[_dbf.Fields.Where(x => x.Name.ToLower() == "schnum").FirstOrDefault()]?.ToString() ?? "";
            var SchoolName = record[_dbf.Fields.Where(x => x.Name.ToLower() == "sch_name").FirstOrDefault()]?.ToString() ?? "";
            var CertNo = record[_dbf.Fields.Where(x => x.Name.ToLower() == "cert_sn").FirstOrDefault()]?.ToString() ?? "";
            var CandidateNo = record[_dbf.Fields.Where(x => x.Name.ToLower() == "reg_no").FirstOrDefault()]?.ToString() ?? "";
            var CandidateName = record[_dbf.Fields.Where(x => x.Name.ToLower() == "cand_name").FirstOrDefault()]?.ToString() ?? "";

            _mdata.Clear();
            _mdata.Append(SchoolNo);
            _mdata.Append(';');
            _mdata.Append(CandidateNo);
            _mdata.Append(';');
            _mdata.Append(CertNo);
            _mdata.Append(';');
            _mdata.Append(CandidateName.Trim());
            _mdata.Append(';');
            _mdata.Append(SchoolName.Trim());
            

            // Define the text or data to encode as a QR code
            string data = _mdata.ToString();

            // Configure the QR code writer
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = 250,
                    Height = 250,
                    Margin = 0
                }
            };

            // Generate the QR code pixel data
            var pixelData = writer.Write(data);

            // Convert the pixel data to a bitmap
            var bitmap = new Bitmap(pixelData.Width, pixelData.Height);
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
            bitmap.UnlockBits(bitmapData);

            // Save the bitmap as a PNG file
            var _fileName = Path.Combine(QRCodePath, CandidateNo);
            _fileName += ".png";
            if (File.Exists(_fileName))
            {
                File.Delete(_fileName);
            }
            bitmap.Save(_fileName, System.Drawing.Imaging.ImageFormat.Png);
        }

        return "done ...";
    });
    

}