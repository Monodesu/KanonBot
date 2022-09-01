// See https://aka.ms/new-console-template for more information
using RosuPP;
try
{
    var ser = Calculator.New("M2U - Lunatic Sky (buhei) [LUNATIC].osu");
    var p = ScoreParams.New();
    p.Mode(Mode.Osu);
    var res = ser.Calculate(p.Context);
    
}
catch (Exception e)
{
    Console.WriteLine(e);
}
// ser.Hello("hello from csharp");
// Console.WriteLine(ser.ReturnString());
