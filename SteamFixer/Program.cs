using System.Diagnostics;

namespace Steam_Fixer
{
    class Program
    {
        static void Main(string[] args)

        {

            string processName = "Steam";

            Process[] processes = Process.GetProcessesByName(processName);



            foreach (Process process in processes)

            {

                process.Kill();


            }
            {
                string process1Name = "RzSynapse";
                Process[] processes1 = Process.GetProcessesByName(process1Name);
                foreach (Process process1 in processes1)
                {

                    process1.Kill();
                }
            }
        }
    }
}