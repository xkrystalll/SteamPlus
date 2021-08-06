using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace SteamPlus
{
    class Program
    {
        private List<string> _gamesAppIds = new List<string>();
        private Dictionary<string, string> _games = new Dictionary<string, string>();
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
            System.IO.File.AppendAllText("gamesAppids.txt", $"\n{appid}");
            Console.WriteLine($"Игра с appid {appid} добавлена!");
            GetGames();
            Main(null);
        }

        private void SwitchGame()
        {
            int i = 1;
            foreach (var x in _games)
            {
                Console.WriteLine($"{i}. {x.Value}");
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
                    RunSteam(_games.ToList()[result - 1].Key);
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
            if (num - 1 <= 0)
            {
                return false;
            }
            return true;
        }

        private void RunSteam(string appid)
        {
            Process process = new Process();
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = path,
                Arguments = $"-noshaders -no-shared-textures -disablehighdpi -cef-single-process -cef-in-process-gpu -single_core -cef-disable-d3d11 -cef-disable-sandbox -disable-winh264 -cef-force-32bit -no-cef-sandbox -cef-disable-breakpad -skipstreamingdrivers -vrdisable -nocrashdialog -nocrashmonitor -norepairfiles -noverifyfiles -nodircheck -nocache -noaafonts -no-dwrite -single_core -voice_quality 1 -nofriendsui -no-browser  -applaunch {appid}"
            };
            process.StartInfo = info;
            process.Start();
        }
        private void GetGameNames()
        {
            _games.Clear();
            foreach (string appid in _gamesAppIds)
            {
                using (var strr = new StreamReader(WebRequest.Create($"https://store.steampowered.com/api/appdetails?appids={appid}").GetResponse().GetResponseStream()))
                {
                    var str = strr.ReadToEnd().ToString();
                    JObject o = JObject.Parse(str);
                    bool success = false;
                    try
                    {
                        success = Convert.ToBoolean(o[appid]["success"].ToString());
                    }
                    catch
                    {
                        success = true;
                    }
                    if (success)
                    {
                        _games.Add(appid, o[appid]["data"]["name"].ToString());
                    }
                    else
                    {
                        _games.Add(appid, appid);
                        continue;
                    }
                }
            }
            program.SwitchGame();
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
            GetGameNames();
        }
    }
}
