using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Support.V7.App;
using Android.Util;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace First_Xamarin_App
{
    [Activity(Name = "com.companyname.first_xamarin_app.ReadBraille", Label = "ReadBraille")]
    public class ReadBraille : AppCompatActivity
    {
        static readonly string LOG_TAG = typeof(TakePicture).Name; //For Debug Log and Error Log

        int KeysIAB;
        TextView textView_displaybraille;
        byte[] braillebyte;
        private byte[,] totalView;
        private int totalrow;
        private int currentRow;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            //set connect device layout
            SetContentView(Resource.Layout.activity_read_braille);
            //return home layout
            Button button0 = FindViewById<Button>(Resource.Id.button0);
            button0.Click += delegate { StartActivity(typeof(MainActivity)); };
            //previous, next of braille bytes
            Button button_previous = FindViewById<Button>(Resource.Id.button_previous);
            Button button_next = FindViewById<Button>(Resource.Id.button_next);
            button_previous.Click += Button_previous_Click;
            button_next.Click += Button_next_Click;

            //show results
            textView_displaybraille = FindViewById<TextView>(Resource.Id.textView_displaybraille);

            string ocrtext = Intent.GetStringExtra("text");
            if (ocrtext != null) {
                Translate(ocrtext);
            }
        }

        private void Button_next_Click(object sender, EventArgs e)
        {
            KeysIAB = 20;
            //textView_displaybraille.Text = KeysIAB.ToString();

            if (braillebyte != null)
            {
                if (currentRow <= totalrow) //if within the range, di lalagpas sa rows, gogogo
                {
                    currentRow = currentRow + 1; //set row
                    DisplayBraille(currentRow);
                    //pause for a while before it reads another key again

                }
                KeysIAB = 0;

            }
        }

        private void Button_previous_Click(object sender, EventArgs e)
        {
            KeysIAB = 1;
            //textView_displaybraille.Text = KeysIAB.ToString();


            if (braillebyte != null)
            {
                if (currentRow >= 0) //if within the range, di bababa sa zero, gogogo
                {
                    currentRow = currentRow - 1; //set row
                    DisplayBraille(currentRow);
                    //pause for a while before it reads another key again

                }
                KeysIAB = 0;

            }
        }

        private void Translate(string text)
        {
            String status = "translating to Braille";
            SpeakStatus(status);

            status = "OCR text reading demo";
            SpeakStatus(status);
            SpeakStatus(text);

            //todo: collect all in enum? indexer?
            String abc = " abcdefghijklmnopqrstuvwxyz,.;:!?";
            String sym = "+ - * ÷ = ( ) ^ ~ & $ @ < > _ % [ ] # | / ";
            String num = " 1234567890";
            char[] alphabet = abc.ToCharArray();
            char[] symbol = sym.ToCharArray();
            char[] number = num.ToCharArray();
            byte[] valuesOfAlphabet = { 0, 128, 192, 144, 152, 136, 208, 216, 200, 80, 88, 160, 224, 176, 184, 168, 240, 248, 232, 112, 120, 164, 228, 92, 180, 188, 172, 64, 76, 96, 72, 104, 100 }; //add (byte) before number if di gumana
            byte[] valuesOfSymbol = { 8, 104, 8, 36, 8, 100, 8, 48, 8, 108, 8, 196, 8, 56, 16, 68, 16, 40, 16, 244, 16, 112, 16, 128, 16, 196, 16, 56, 20, 36, 20, 44, 20, 196, 20, 56, 28, 148, 28, 204, 28, 132 };

            char[] arraytext = text.ToCharArray();   //ocr text from take picture

            //translate

            braillebyte = new byte[arraytext.Length];

            int k = 0;
            for (int i = 0; i < arraytext.Length; i++)
            {
                for (int j = 0; j < symbol.Length; j++)
                {
                    if (j < alphabet.Length) //check only within the bounds of alphabet array
                    {
                        bool IsCapslock = (arraytext[i].ToString()).Equals(alphabet[j].ToString(), StringComparison.OrdinalIgnoreCase);
                        //case sensitive, lowercase only
                        if (arraytext[i] == alphabet[j])
                        {
                            int ignoreMe;
                            if (i != 0)
                            {
                                if (int.TryParse(arraytext[i - 1].ToString(), out ignoreMe) == false)
                                {
                                    //if before char is not a number, proceed as usual
                                    braillebyte[k] = valuesOfAlphabet[j];
                                    k++;
                                    break; //once it finds what it needs, it stops scanning, then scans for the next char
                                }
                                else if (int.TryParse(arraytext[i - 1].ToString(), out ignoreMe))
                                {
                                    //if before char is a number
                                    //check if the char now is a-j
                                    //if it is, add letter sign before it                                
                                    if (arraytext[i] >= 'a' && arraytext[i] <= 'j')
                                    {
                                        Array.Resize(ref braillebyte, braillebyte.Length + 1); //adds length than initial
                                        braillebyte[k] = 12; //byte for letter sign (force stop of number sign)
                                        k++;
                                        braillebyte[k] = valuesOfAlphabet[j];
                                        k++;
                                        break;
                                    }
                                    else
                                    {
                                        //else if k-z
                                        braillebyte[k] = valuesOfAlphabet[j];
                                        k++;
                                        break;
                                    }
                                }
                            }
                            //if initial
                            braillebyte[k] = valuesOfAlphabet[j];
                            k++;
                            break;
                        }
                        //case insensitive, compare upper with lower case                 
                        else if (IsCapslock)
                        {
                            int ignoreMe;
                            //if before char is not a number, proceed as usual
                            if (i != 0)
                            {
                                if (int.TryParse(arraytext[i - 1].ToString(), out ignoreMe) == false)
                                {
                                    Array.Resize(ref braillebyte, braillebyte.Length + 1);
                                    braillebyte[k] = 4; //byte for capslock indicator
                                    k++;
                                    braillebyte[k] = valuesOfAlphabet[j];
                                    k++;
                                    break;
                                }

                                else if (int.TryParse(arraytext[i - 1].ToString(), out ignoreMe))
                                {
                                    //if before char is a number
                                    //check if the char now is A-J
                                    //if it is, add letter sign before it                                
                                    if (arraytext[i] >= 'A' && arraytext[i] <= 'J')
                                    {
                                        Array.Resize(ref braillebyte, braillebyte.Length + 2);
                                        braillebyte[k] = 12; //byte for letter sign (force stop of number sign)
                                        k++;
                                        braillebyte[k] = 4; //byte for capslock indicator
                                        k++;
                                        braillebyte[k] = valuesOfAlphabet[j];
                                        k++;
                                        break;
                                    }
                                    else
                                    {
                                        //else if k-z
                                        Array.Resize(ref braillebyte, braillebyte.Length + 1);
                                        braillebyte[k] = 4; //byte for capslock indicator
                                        k++;
                                        braillebyte[k] = valuesOfAlphabet[j];
                                        k++;
                                        break;
                                    }
                                }
                            }
                            //if initial
                            Array.Resize(ref braillebyte, braillebyte.Length + 1);
                            braillebyte[k] = 4; //byte for capslock indicator
                            k++;
                            braillebyte[k] = valuesOfAlphabet[j];
                            k++;
                            break;
                        }                    
                    }

                    if (j < number.Length) //check only within the bounds of number array
                    {
                        if (arraytext[i] == number[j])
                        {
                            int ignoreMe;
                            //if before char is a number (or comma, colon, hyphen, fraction, decimal)
                            if (i != 0)
                            {
                                if (int.TryParse(arraytext[i - 1].ToString(), out ignoreMe))
                                {
                                    braillebyte[k] = valuesOfAlphabet[j]; //don't add number indicator
                                    k++;
                                    break;
                                }
                                //if before char is not a number or is equal to space
                                else if ((int.TryParse(arraytext[i - 1].ToString(), out ignoreMe) == false) || (arraytext[i - 1].ToString() == " "))
                                {
                                    Array.Resize(ref braillebyte, braillebyte.Length + 1);
                                    braillebyte[k] = 60; //add byte for number indicator
                                    k++;
                                    braillebyte[k] = valuesOfAlphabet[j];
                                    k++;
                                    break;
                                }
                            }
                            //if initial
                            Array.Resize(ref braillebyte, braillebyte.Length + 1);
                            braillebyte[k] = 60; //add byte for number indicator
                            k++;
                            braillebyte[k] = valuesOfAlphabet[j];
                            k++;
                            break;
                        }
                    }

                    if (arraytext[i] == symbol[j])
                    {
                        Array.Resize(ref braillebyte, braillebyte.Length + 1);
                        braillebyte[k] = valuesOfSymbol[j];
                        k++;
                        braillebyte[k] = valuesOfSymbol[j + 1];
                        k++;
                        break;
                    }

                }
            }

            //divide huge chunk of braille bytes into 20
            //row depends on how long the message is, column always 20. always round up to ceiling
            totalrow = Convert.ToInt32(Math.Ceiling(braillebyte.Length / 20.0));
            totalView = new byte[totalrow, 20];
            int z = 0;
            //assign to totalView
            for (int x = 0; x < totalrow; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    if (z < braillebyte.Length)
                    {
                        totalView[x, y] = braillebyte[z];
                        z++;
                    }
                }
            }

            if (totalView != null)
            {
                currentRow = 0;
                DisplayBraille(currentRow);
            }
        }

        private void DisplayBraille(int currow)
        {
            //interfacing the braille bytes
            byte[] currentView = new byte[20];

            if (currow >= totalrow || currow < 0) { return; }

            for (int s = 0; s < 20; s++)
            {
                currentView[s] = totalView[currow, s];

            }
            //_device.Send(currentView);
            if (currentView != null)
            {
                BrailleinTextView(currentView);
            }
        }

        private void BrailleinTextView(byte[] currentView)
        {
            textView_displaybraille.Text = BitConverter.ToString(currentView);
        }

        private async void SpeakStatus(string status)
        {
            //voice indicator
            await Task.Run(async () =>
            {
                await TextToSpeech.SpeakAsync(status);
            });
        }

        void DebugLog(string msg)
        {
            Log.Debug(LOG_TAG, msg);
        }
    }
}