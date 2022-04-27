using EscPrinterTest.Printer;
using System.Reflection;

const string ipAddress = "192.168.1.240";
const int portNumber = 9100;

EscPrinter printer = new EscPrinter(ipAddress, portNumber);

string currentExecutionPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
string imagePath = Path.Combine(currentExecutionPath, "imagePng80q5x.Png");

await printer.Print(imagePath);

Console.WriteLine("Image printed successfully !!");
Console.ReadKey();
