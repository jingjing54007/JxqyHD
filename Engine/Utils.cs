﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    static public class Utils
    {
        //static public int GetBigEndianIntegerFromByteArray(byte[] data, ref int startIndex)
        //{
        //    var ret = (data[startIndex] << 24)
        //         | (data[startIndex + 1] << 16)
        //         | (data[startIndex + 2] << 8)
        //         | data[startIndex + 3];
        //    startIndex += 4;
        //    return ret;
        //}

        static public int GetLittleEndianIntegerFromByteArray(byte[] data, ref int startIndex)
        {
            var ret = (data[startIndex + 3] << 24)
                 | (data[startIndex + 2] << 16)
                 | (data[startIndex + 1] << 8)
                 | data[startIndex];
            startIndex += 4;
            return ret;
        }

        //return string[0] is name, string[1] is value
        //nameValue patten is "name=value"
        //If failed return empty string array
        static public string[] GetNameValue(string nameValue)
        {
            var result = new String[2];
            var groups = Regex.Match(nameValue, "(.*)=(.*)").Groups;
            if (groups[0].Success)
            {
                result[0] = groups[1].Value;
                result[1] = groups[2].Value;
            }
            return result;
        }

        static public Asf GetAsf(string path)
        {
            try
            {
                var hashCode = path.GetHashCode();
                if (Globals.AsfFiles.ContainsKey(hashCode))
                    return Globals.AsfFiles[hashCode];
                else
                {
                    var asf = new Asf(path);
                    if (asf.IsOk)
                    {
                        Globals.AsfFiles[hashCode] = asf;
                        return asf;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        static public SoundEffect GetSoundEffect(string wavFileName)
        {
            try
            {
                var groups = Regex.Match(wavFileName, @"(.+)\.wav").Groups;
                string assertName = wavFileName;
                if (groups[0].Success)
                {
                    assertName = @"sound\" + groups[1].Value;
                }
                return Globals.TheGame.Content.Load<SoundEffect>(assertName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        static public Magic GetMagic(string filePath)
        {
            try
            {
                var hashCode = filePath.GetHashCode();
                if (Globals.Magics.ContainsKey(hashCode))
                    return Globals.Magics[hashCode];
                else
                {
                    var magic = new Magic(filePath);
                    if (magic.IsOk)
                    {
                        Globals.Magics[hashCode] = magic;
                        return magic;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static Dictionary<int, LevelDetail> GetLevelLists(string filePath)
        {
            var lists = new Dictionary<int, LevelDetail>();

            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.GetEncoding(Globals.SimpleChinaeseCode));
                var counts = lines.Length;
                for (var i = 0; i < counts; )
                {
                    var groups = Regex.Match(lines[i], @"\[Level([0-9]+)\]").Groups;
                    i++;
                    if (groups[0].Success)
                    {
                        var detail = new LevelDetail();
                        var index = int.Parse(groups[1].Value);
                        while (i < counts && !string.IsNullOrEmpty(lines[i]))
                        {
                            var nameValue = GetNameValue(lines[i]);
                            int value;
                            int.TryParse(nameValue[1], out value);
                            switch (nameValue[0])
                            {
                                case "LevelUpExp":
                                    detail.LevelUpExp = value;
                                    break;
                                case "LifeMax":
                                    detail.LifeMax = value;
                                    break;
                                case "ThewMax":
                                    detail.ThewMax = value;
                                    break;
                                case "ManaMax":
                                    detail.ManaMax = value;
                                    break;
                                case "Attack":
                                    detail.Attack = value;
                                    break;
                                case "Defend":
                                    detail.Defend = value;
                                    break;
                                case "Evade":
                                    detail.Evade = value;
                                    break;
                                case "NewMagic":
                                    detail.NewMagic = nameValue[1];
                                    break;
                            }
                            i++;
                        }
                        lists[index] = detail;
                    }
                }

            }
            catch (Exception)
            {
                return lists;
            }

            return lists;
        }

        //axes:x point to (1,0), y point to is (1,0) in game axes
        //direction: 0-31 clockwise, 0 point to (0,1)
        private static List<Vector2> _direction32List; 
        public static Vector2 GetDirection32(int direction)
        {
            if (direction < 0 || direction > 31)
                direction = 0;
            return GetDirection32List()[direction];
        }

        public static List<Vector2> GetDirection32List()
        {
            if (_direction32List == null)
            {
                _direction32List = new List<Vector2>();
                var angle = Math.PI * 2 / 32;
                for (var i = 0; i < 32; i++)
                {
                    _direction32List.Add(new Vector2((float)-Math.Sin(angle * i), (float)Math.Cos(angle * i))); ;
                }
            }
            return _direction32List;
        }

        //Please see ../Helper/SetDirection.jpg
        public static int GetDirection(Vector2 direction, int directionCount)
        {
            if (direction == Vector2.Zero || directionCount < 1) return 0;
            const double twoPi = Math.PI * 2;
            direction.Normalize();
            var angle = Math.Acos(Vector2.Dot(direction, new Vector2(0, 1)));
            if (direction.X > 0) angle = twoPi - angle;

            // 2*PI/2*directionCount
            var halfAnglePerDirection = Math.PI / directionCount;
            var region = (int)(angle / halfAnglePerDirection);
            if (region % 2 != 0) region++;
            region %= 2 * directionCount;
            return region / 2;
        }

        public struct LevelDetail
        {
            public int LevelUpExp;
            public int LifeMax;
            public int ThewMax;
            public int ManaMax;
            public int Attack;
            public int Defend;
            public int Evade;
            public string NewMagic;
        }
    }
}
