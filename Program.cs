using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

internal class Program {
    [DllImport("user32.dll")]
    static extern int GetAsyncKeyState(Int32 i);

    static void SetConnection(bool Enabled = true) {
        ProcessStartInfo proi = new ProcessStartInfo {
            FileName = "ipconfig",
            Arguments = Enabled ? "/renew" : "/release",
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };
        Process proc = Process.Start(proi);
        proc.WaitForExit();
        proc.Dispose();
    }

    static void Main(string[] args) {
        int key = 66;
        float delay = 5000;
        int mode = 0;

        if (File.Exists("lagswitch.op")) {
            string[] stt = File.ReadAllText("lagswitch.op").Split('|');
            key = Convert.ToInt32(stt[0]);
            delay = Convert.ToInt32(stt[1]);
            mode = Convert.ToInt32(stt[2]);
        }

        while (true) {
            string mstr = mode == 0 ? "Toggle" : mode == 1 ? "Held" : "Delay";
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine($"1. Current key: {(char)key}\n2. Mode: {mstr}\n3. Run"+(mode==2 ? $"\n4. Current delay: {delay/1000}" : "")+"\nESC. Exit");
            ConsoleKeyInfo ck = Console.ReadKey(true);
            if (ck.KeyChar == '1') {
                Console.Clear();
                Console.Write($"ESC. Exit\n\nCurrent key: {(char)key}\nNew key: ");

                ConsoleKeyInfo nk = Console.ReadKey();
                if (nk.KeyChar != '\0' && nk.KeyChar != '\x1b') {
                    key = nk.KeyChar.ToString().ToUpper().ToCharArray()[0];
                    Thread.Sleep(1000);
                } else if (nk.Key == ConsoleKey.Escape) break;

            } else if (ck.KeyChar == '2') {
                mode = (mode + 1) % 3;
            } else if (ck.KeyChar == '3') {
                Console.Clear();
                Console.WriteLine("ESC. Stop");
                bool oldState = false;
                bool connected = true;
                while (true) {
                    Thread.Sleep(5);
                    if (Console.KeyAvailable) {
                        ConsoleKeyInfo rk = Console.ReadKey(true);
                        if (rk.Key == ConsoleKey.Escape) break;
                    }
                    bool held = GetAsyncKeyState(key) > 32767;
                    
                    if (held != oldState) {
                        if (mode == 0 && held) {
                            connected = !connected;
                            SetConnection(connected);
                        } else if (mode == 1) {
                            SetConnection(!held);
                        } else if (mode == 2 && held) {
                            SetConnection(false);
                            Thread.Sleep((int)delay);
                            SetConnection(true);
                        }
                    }
                    oldState = held;
                }
                Console.WriteLine("Resetting, please wait...");
                SetConnection(true);
            } else if (ck.KeyChar == '4') {
                Console.Clear();
                Console.Write($"ESC. Exit\n\nCurrent delay: {delay/1000}\nType new delay (in seconds): ");
                string input = Console.ReadLine();
                float newdel;
                if (float.TryParse(input, out newdel)) {
                    delay = newdel*1000;
                } else {
                    Console.WriteLine("Error, try again.");
                    Thread.Sleep(1000);
                }
            } else if (ck.Key == ConsoleKey.Escape) break;
        }

        File.WriteAllText("lagswitch.op", $"{key}|{delay}|{mode}");
    }
}