using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace StonedSeriesAIO
{
    internal class Program
    {
       private static void Main(string[] args)
        {
           string ChampionSwitch = ObjectManager.Player.ChampionName.ToLowerInvariant();

           switch(ChampionSwitch)
           {
               case "amumu":
                new Amumu();
                Game.PrintChat("<font color='#FF00BF'>Stoned 鍧﹀厠鍚堥泦 |闃挎湪鏈▅ {0} 鍔犺級鎴愬姛!婕㈠寲by浜岀嫍!QQ缇361630847! By</font> <font color='#FF0000'>The</font><font color='#FFFF00'>Kush Style</font><font color='#40FF00'>婕㈠寲by浜岀嫍!QQ缇361630847!</font>",ChampionSwitch);
                break;

               case "drmundo":
                new DrMundo();
                Game.PrintChat("<font color='#FF00BF'>Stoned 鍧﹀厠鍚堥泦 |钂欏|{0} 鍔犺級鎴愬姛!婕㈠寲by浜岀嫍!QQ缇361630847! By</font> <font color='#FF0000'>The</font><font color='#FFFF00'>Kush Style</font><font color='#40FF00'>婕㈠寲by浜岀嫍!QQ缇361630847!</font>",ChampionSwitch);
                break;

              case "jarvaniv":
                new JarvanIV();
                Game.PrintChat("<font color='#FF00BF'>Stoned 鍧﹀厠鍚堥泦 |鐜嬪瓙|{0} 鍔犺級鎴愬姛!婕㈠寲by浜岀嫍!QQ缇361630847! By</font> <font color='#FF0000'>The</font><font color='#FFFF00'>Kush Style</font><font color='#40FF00'>婕㈠寲by浜岀嫍!QQ缇361630847!</font>",ChampionSwitch);
                break;
                
               case "volibear":
                new Volibear();
                Game.PrintChat("<font color='#FF00BF'>Stoned 鍧﹀厠鍚堥泦 |闆烽渾鍜嗗摦|{0} 鍔犺級鎴愬姛!婕㈠寲by浜岀嫍!QQ缇361630847! By</font> <font color='#FF0000'>The</font><font color='#FFFF00'>Kush Style</font><font color='#40FF00'>婕㈠寲by浜岀嫍!QQ缇361630847!</font>",ChampionSwitch);
                break;

               default:
                Game.PrintChat("{0} not supported in Stoned Series",ChampionSwitch);
                   break;
           }
           
           
        }
    }
}
