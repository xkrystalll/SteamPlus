using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SteamPlus
{
    class Program
    {
        private List<string> _gamesAppIds = new List<string>();
        string path = "";
        private static Program program = new Program();
        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(program.path))
            {
                Console.WriteLine("Введите путь к файлу steam.exe");
                string mayPath = Console.ReadLine();
                if (!File.Exists(mayPath) || !mayPath.Contains("steam.exe"))
                {
                    Console.WriteLine("Вы ввели неверный путь к steam.exe");
                    Main(null);
                    return;
                }
                program.path = mayPath;
            }

            program.GetGames();
            program.SwitchGame();
        }

        private void AddGame()
        {
            Console.WriteLine("Введите appid игры:");
            string appid = Console.ReadLine();
            File.AppendAllText("gamesAppids.txt", $"\n{appid}");
            Console.WriteLine($"Игра с appid {appid} добавлена!");
            GetGames();
            Main(null);
        }

        private void SwitchGame()
        {
            int i = 1;
            foreach (var x in _gamesAppIds)
            {
                Console.WriteLine($"{i}. {x}");
                i++;
            }
            Console.WriteLine("-1. Добавить новую игру");
            DoChoose();
        }

        private void DoChoose()
        {
            Console.WriteLine("Выберите игру из списка выше (цифровой номер):");
            string game = Console.ReadLine();
            if (game == "-1")
            {
                AddGame();
                return;
            }
            if (int.TryParse(game, out int result))
            {
                if (CheckValidSwitchedGame(result))
                {
                    RunSteam(_gamesAppIds[result - 1]);
                }
                else
                {
                    Console.WriteLine($"Игра с индексом {result} не найдена.");
                    Main(null);
                    return;
                }
            }
        }

        private bool CheckValidSwitchedGame(int num)
        {
            if (_gamesAppIds.Count < num)
            {
                return false;
            }
            if (num - 1 < 0)
            {
                return false;
            }
            return true;
        }

        private void KillOldSteamProcess()
        {
            List<string> name = new List<string> { "steam" };
            Process[] etc = Process.GetProcesses();
            foreach (Process anti in etc)
            {
                foreach (string s in name)
                {
                    if (anti.ProcessName.ToLower().Contains(s.ToLower()))
                    {
                        anti.Kill();
                    }
                }
            }
        }

        private void RunSteam(string appid)
        {
            KillOldSteamProcess();

            Process process = new Process();
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = path,
                Arguments = $"-noshaders -no-shared-textures -disablehighdpi -cef-single-process -cef-in-process-gpu -single_core -cef-disable-d3d11 -cef-disable-sandbox -disable-winh264 -cef-force-32bit -no-cef-sandbox -cef-disable-breakpad -skipstreamingdrivers -vrdisable -nocrashdialog -nocrashmonitor -norepairfiles -noverifyfiles -nodircheck -nocache -noaafonts -no-dwrite -single_core -voice_quality 1 -nofriendsui -no-browser  -applaunch {appid}"
            };
            process.StartInfo = info;
            process.Start();
        }

        private void GetGames()
        {
            try
            {
                _gamesAppIds.Clear();
                using (FileStream stream = File.Open("gamesAppids.txt", FileMode.OpenOrCreate, FileAccess.Read))
                {
                    byte[] array = new byte[stream.Length];
                    stream.Read(array, 0, array.Length);
                    string textFromFile = System.Text.Encoding.Default.GetString(array);
                    foreach (var str in textFromFile.Split('\n'))
                    {
                        if (!string.IsNullOrEmpty(str))
                            _gamesAppIds.Add(str);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Файл занят другим процессом. (Открыт проводник?)");
                Main(null);
                return;
            }
        }
    }
}
