using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace JojaExpress.PhoneScreen
{
    internal class DialoguePhoneScreen : PhoneScreenBase
    {
        public string dialogues;
        public int characterPosition;

        public DialoguePhoneScreen(string dialogues, MenuPhoneScreen father) 
        {
            this.dialogues = dialogues;
            characterPosition = 0;
            this.father = father;
        }

        public override PhoneScreenBase? click(int x, int y)
        {
            if (characterPosition != dialogues.Length)
            {
                characterPosition = dialogues.Length;
                return this;
            }
            else return father;
        }

        public override void drawLandscape(SpriteBatch b, int xOff, int yOff)
        {
            drawLandscapeBackground(b, xOff, yOff, true);
            MobilePhoneRender.drawStr(b, dialogues[..characterPosition], new(xOff + 40, yOff + 25, 400, 240), Game1.smallFont, false);
        }

        public override void drawProtrait(SpriteBatch b, int xOff, int yOff)
        {
            drawProtraitBackground(b, xOff, yOff, true);
            MobilePhoneRender.drawStr(b, dialogues[..characterPosition], new(xOff + 25, yOff + 40, 240, 400), Game1.smallFont, false);
        }
    }
}
