#region

using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion


namespace Mata_View
{
    public class ListedText
    {
        public int NetworkId;
        public string Name;
        public float Duration;
        public Color ObjColor;
        public int Range;
        public Vector3 Position;
        public float CreatedAt;
        public static GameObject SenderN;
        public int RealtimeCheck;
        public Obj_AI_Hero Heroname;

        public bool Visible = true;

        public Render.Text Timer { get; set; }
        public ListedText(int netId, string name, float duration, Vector3 position, float createdAt, GameObject senderN, int realtimecheck, Obj_AI_Hero heroname)
        {
            NetworkId = netId;
            Name = name;
            Duration = duration;
            Position = position;
            CreatedAt = createdAt;

            SenderN = senderN;
            RealtimeCheck = realtimecheck;
            Heroname = heroname;

            Timer = new Render.Text("", new Vector2(0, 0), (Menus.testsize.GetValue<Slider>().Value) * 2, SharpDX.Color.White)
            {
                VisibleCondition =
                condition => (((Duration == -1 || (int)((CreatedAt + Duration + 1) - Game.Time) > 0) && (duration) > 0 && Visible)),
                 
                PositionUpdate = delegate
                {
                    var pos = Drawing.WorldToScreen(new Vector3(Position.X, Position.Y, Position.Z));
                    switch (RealtimeCheck)
                    {
                        case 0: //Object First Created Posistion

                            break;
                        case 1: //Realtime on Hero Position
                            Position = Heroname.Position;
                            Position.Y += 80f; // I don't know why it needs, but if I don't add posistion it won't draw timer
                            break;
                        case 2: //Realtime on Sender Position
                            Position = SenderN.Position;
                            Position.Y += 40f;
                            break;

                    }
                    return pos;
                },
                TextUpdate = () => ((CreatedAt + Duration) - Game.Time).ToString("0.0"),
                OutLined = true,
                Centered = true
            };
            Timer.Add();
        }

       
    }

   
    
}
