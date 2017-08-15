using System.Diagnostics;

namespace Steam_Fixer
{
    class Program
    {
        static void Main(string[] args)

        {

            string processName = "Frzestat2k";

            Process[] processes = Process.GetProcessesByName(processName);



            foreach (Process process in processes)

            {

                process.Kill();


            }
                {
                    string process2Name = "Student";
                    Process[] processes2 = Process.GetProcessesByName(process2Name);


                    foreach (Process process2 in processes2)
                    {

                        process2.Kill();
                    }
                }
            }
        }
    }