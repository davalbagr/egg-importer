using System;
using System.IO;
using System.Net;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using PKHeX.Core;
using System.Drawing;

namespace EggImporter
{

    public class EggImporterPlugin : IPlugin
    {
        public string Name => nameof(EggImporter);
        public int Priority => 1; // Loading order, lowest is first.

        // Initialized on plugin load
        public ISaveFileProvider SaveFileEditor { get; private set; } = null!;
        public IPKMView PKMEditor { get; private set; } = null!;

        //Random number generator for form generation
        Random random = new Random();

        // Forms
        private Form form = new Form();
        private TextBox numbgen = new TextBox();
        private TextBox eggchance = new TextBox();
        private TextBox hiddenchance = new TextBox();
        private TextBox shinychance = new TextBox();
        private CheckBox maxivs = new CheckBox();
        public Label label1 = new Label();
        public Label label2 = new Label();
        public Label label3 = new Label();
        public Label label4 = new Label();
        public Label label5 = new Label();

        //Pokemon forms
        public int[] burmyForms = { 412, 905, 906 };
        public int[] shellosForms = { 422, 911 };
        public int[] scatterbugForms = { 666, 963, 964, 965, 966, 967, 968, 969, 970, 971, 972, 973, 974, 975, 976, 977, 978, 979, 980, 981 };
        public int[] flabebeForms = { 669, 986, 987, 988, 989 };
        public int[] oricorioForms = { 741, 1021, 1022, 1023 };
        public int[] miniorForms = { 774, 1045, 1046, 1047, 1048, 1049, 1050, 1051, 1052, 1053, 1054, 1055, 1056, 1057};

        public static string FirstLetterToUpper(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        // Plugin initialization
        public void Initialize(params object[] args)
        {
            Console.WriteLine($"Loading {Name}...");
            SaveFileEditor = (ISaveFileProvider)Array.Find(args, z => z is ISaveFileProvider);
            PKMEditor = (IPKMView)Array.Find(args, z => z is IPKMView);
            var menu = (ToolStrip)Array.Find(args, z => z is ToolStrip);
            LoadMenuStrip(menu);
        }

        private void load_pokemons(string pks)
        {
            var sav = SaveFileEditor.SAV; // current save
            int generation = sav.Generation; // the generation of the current save -- used to determine the PK save format
            List<PKM> pokemonList = new List<PKM>(); // list of all Pokemon that will be added to the sav
            GameVersion game = (GameVersion)sav.Game;
            List<RawPokemon>? pokemons = JsonConvert.DeserializeObject<List<RawPokemon>>(pks);
            if (pokemons == null)
            {
                return;
            }
            foreach (RawPokemon rawPokemon in pokemons)
            {
                
                PKM pokemon = new PK2();

                //Determine save format
                switch (generation)
                {
                    case 1:
                        pokemon = new PK1();
                        break;
                    case 2:
                        pokemon = new PK2();
                        break;
                    case 3:
                        pokemon = new PK3();
                        break;
                    case 4:
                        pokemon = new PK4();
                        break;
                    case 5:
                        pokemon = new PK5();
                        break;
                    case 6:
                        pokemon = new PK6();
                        break;
                    case 7:
                        pokemon = new PK7();
                        break;
                    case 8:
                        pokemon = new PK8();
                        break;
                }

                pokemon.Species = rawPokemon.Species;

                // Check to see if the Pokemon has forms (i.e. Flabebe, Shellos, etc.), and if it does, randomly generate one
                if (rawPokemon.Species == 412)
                {
                    int form = random.Next(0, burmyForms.Length - 1);
                    pokemon.SetForm(form);
                }
                else if (rawPokemon.Species == 422)
                {
                    int form = random.Next(0, shellosForms.Length - 1);
                    pokemon.SetForm(form);
                }
                else if (rawPokemon.Species == 664)
                {
                    int form = random.Next(0, scatterbugForms.Length - 1);
                    pokemon.SetForm(form);
                }
                else if (rawPokemon.Species == 669)
                {
                    int form = random.Next(0, flabebeForms.Length - 1);
                    pokemon.SetForm(form);
                }
                else if (rawPokemon.Species == 741)
                {
                    int form = random.Next(0, oricorioForms.Length - 1);
                    pokemon.SetForm(form);
                }
                else if (rawPokemon.Species == 774)
                {
                    int form = random.Next(0, miniorForms.Length - 1);
                    pokemon.SetForm(form);
                }

                EncounterEgg encounterEgg = new EncounterEgg(rawPokemon.Species, pokemon.Form, 1, sav.Generation, game);
                PKM pokemonAsEgg = encounterEgg.ConvertToPKM(sav);
                pokemonAsEgg.IsEgg = true;
                pokemon.IsNicknamed = true;
                pokemon.Nickname = "Egg";

                pokemonAsEgg.SetAbility(rawPokemon.Ability);
                pokemonAsEgg.SetGender(rawPokemon.Gender);
                pokemonAsEgg.Nature = rawPokemon.Nature;
                pokemonAsEgg.IV_HP = rawPokemon.HP;
                pokemonAsEgg.IV_ATK = rawPokemon.Atk;
                pokemonAsEgg.IV_DEF = rawPokemon.Def;
                pokemonAsEgg.IV_SPA = rawPokemon.SpA;
                pokemonAsEgg.IV_SPD = rawPokemon.SpD;
                pokemonAsEgg.IV_SPE = rawPokemon.Spe;
                pokemonAsEgg.Move1 = rawPokemon.MoveOne;
                pokemonAsEgg.RelearnMove1 = pokemonAsEgg.Move1;
                pokemonAsEgg.Move2 = rawPokemon.MoveTwo;
                pokemonAsEgg.RelearnMove2 = pokemonAsEgg.Move2;
                pokemonAsEgg.Move3 = rawPokemon.MoveThree;
                pokemonAsEgg.RelearnMove3 = pokemonAsEgg.Move3;
                pokemonAsEgg.Move4 = rawPokemon.MoveFour;
                pokemonAsEgg.RelearnMove4 = pokemonAsEgg.Move4;
                pokemonAsEgg.SetMaximumPPCurrent();

                if (rawPokemon.IsShiny)
                {
                    CommonEdits.SetShiny(pokemonAsEgg);
                }
                else
                {
                    CommonEdits.SetUnshiny(pokemonAsEgg);
                }

                pokemonAsEgg.Met_Location = 0;

                if (generation == 7 || generation == 6 || generation == 5)
                {
                    pokemonAsEgg.Egg_Location = Locations.Daycare5;
                }
                else
                {
                    pokemonAsEgg.Egg_Location = Locations.Daycare4;
                }

                pokemonAsEgg.IsNicknamed = true;
                pokemonAsEgg.Nickname = SpeciesName.GetSpeciesNameGeneration(0, sav.Language, generation);

                //Hatch counter is for some reason called "CurrentFriendship".  Don't ask me why, I don't know.
                pokemonAsEgg.CurrentFriendship = 1;

                pokemonList.Add(pokemonAsEgg);
            }

            // Import Pokemon, reload the boxes so they can be seen, show a message and close the window
            sav.ImportPKMs(pokemonList);
            SaveFileEditor.ReloadSlots();
            MessageBox.Show("Done!");
        }

        // Adding Plugin to PKHeX menu
        private void LoadMenuStrip(ToolStrip menuStrip)
        {
            var items = menuStrip.Items;
            if (!(items.Find("Menu_Tools", false)[0] is ToolStripDropDownItem tools))
                throw new ArgumentException(nameof(menuStrip));
            AddPluginControl(tools);
        }

        // Creating additional controls for the menu
        private void AddPluginControl(ToolStripDropDownItem tools)
        {
            var ctrl = new ToolStripMenuItem(Name);
            tools.DropDownItems.Add(ctrl);

            var c = new ToolStripMenuItem($"Import Pokemon Eggs");
            c.Click += (sender, eventArgs) => generateForm();
            ctrl.DropDownItems.Add(c);
            Console.WriteLine($"{Name} added menu items.");
        }


        public void generateForm()
        {
            Button createButton = new Button();

            this.form.Size = new Size(500, 500);
            this.form.Name = "Bulk Importer";

            this.numbgen.AcceptsReturn = true;
            this.numbgen.AcceptsTab = true;
            this.numbgen.Size = new Size(50, 50);
            this.numbgen.Location = new Point(260, 140);
            this.label1.Text = "Number of pokemon to generate";
            this.label1.AutoSize = true;
            this.label1.Location = new Point(70, 140);
            this.label1.Anchor = AnchorStyles.Left;
            this.label1.TextAlign = ContentAlignment.MiddleLeft;


            this.eggchance.AcceptsReturn = true;
            this.eggchance.AcceptsTab = true;
            this.eggchance.Size = new Size(50, 50);
            this.eggchance.Location = new Point(260, 200);
            this.label2.Text = "Chance pokemon has egg move";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(70, 200);
            this.label2.Anchor = AnchorStyles.Left;
            this.label2.TextAlign = ContentAlignment.MiddleLeft;

            this.hiddenchance.AcceptsReturn = true;
            this.hiddenchance.AcceptsTab = true;
            this.hiddenchance.Size = new Size(50, 50);
            this.hiddenchance.Location = new Point(260, 260);
            this.label3.Text = "Chance pokemon has a hidden ability";
            this.label3.AutoSize = true;
            this.label3.Location = new Point(70, 260);
            this.label3.Anchor = AnchorStyles.Left;
            this.label3.TextAlign = ContentAlignment.MiddleLeft;

            this.shinychance.AcceptsReturn = true;
            this.shinychance.AcceptsTab = true;
            this.shinychance.Size = new Size(50, 50);
            this.shinychance.Location = new Point(260, 320);
            this.label4.Text = "Chance pokemon is shiny";
            this.label4.AutoSize = true;
            this.label4.Location = new Point(70, 320);
            this.label4.Anchor = AnchorStyles.Left;
            this.label4.TextAlign = ContentAlignment.MiddleLeft;

            createButton.Text = "Add to Boxes";
            createButton.Size = new Size(185, 20);
            createButton.Location = new Point(150, 420);
            createButton.Click += new EventHandler(AddToBoxesButtonClick);
            this.maxivs.Location = new Point(260, 360);
            this.maxivs.AutoSize = true;
            this.maxivs.Appearance = Appearance.Normal;
            this.maxivs.AutoCheck = true;
            this.label5.Text = "Should eggs have max ivs";
            this.label5.AutoSize = true;
            this.label5.Anchor = AnchorStyles.Left;
            this.label5.Location = new Point(70, 360);
            this.label5.TextAlign = ContentAlignment.MiddleLeft;

            this.form.Controls.Add(this.numbgen);
            this.form.Controls.Add(this.eggchance);
            this.form.Controls.Add(this.hiddenchance);
            this.form.Controls.Add(this.shinychance);
            this.form.Controls.Add(this.label1);
            this.form.Controls.Add(this.label2);
            this.form.Controls.Add(this.label3);
            this.form.Controls.Add(this.label4);
            this.form.Controls.Add(this.label5);
            this.form.Controls.Add(createButton);
            this.form.Controls.Add(maxivs);
            this.form.ShowDialog();
        }

        public string game_id_to_string(int game)
        {
            switch (game)
            {
                case 1: case 2: case 50:
                    return "ruby-sapphire";
                case 3:
                    return "emerald";
                case 4: case 5:
                    return "firered-leafgreen";
                case 7: case 8: case 58:
                    return "heartgold-soulsilver";
                case 10: case 11: case 56:
                    return "diamond-pearl";
                case 12: case 57:
                    return "platinum";
                case 20: case 21: case 60:
                    return "black-white";
                case 22: case 23: case 61:
                    return "black-2-white-2";
                case 24: case 25: case 62:
                    return "x-y"; 
                case 26: case 27:
                    return "omega-ruby-alpha-sapphire";
                case 30: case 31: case 65:
                    return "ultra-sun-ultra-moon";
                case 44: case 45: case 68:
                    return "sword-shield";
                case 35: case 37: case 46: case 47:
                    return "red-blue";
                case 36:
                    return "green";
                case 38:
                    return "yellow";
                default:
                    return "";

            }
        }

        public string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public void AddToBoxesButtonClick(Object sender, EventArgs events)
        {
            string url = "";
            if (this.maxivs.Checked)
            {
                url = "https://egggen-api.gigalixirapp.com/maxivs/";
            }
            else
            {
                url = "https://egggen-api.gigalixirapp.com/";
            }
            int numbtogen = Int32.Parse(this.numbgen.Text);
            int eggchance = Int32.Parse(this.eggchance.Text);
            int shinychance = Int32.Parse(this.shinychance.Text);
            int hiddenchance = Int32.Parse(this.hiddenchance.Text);
            string game = game_id_to_string(SaveFileEditor.SAV.Game);
            url = url + numbtogen + "/" + game + "/" + eggchance + "/" + hiddenchance + "/" + shinychance;
            string response = Get(url);
            load_pokemons(response);
            this.form.Close();
        }

        public void NotifySaveLoaded()
        {
            Console.WriteLine($"{Name} was notified that a Save File was just loaded.");
        }

        public bool TryLoadFile(string filePath)
        {
            Console.WriteLine($"{Name} was provided with the file path, but chose to do nothing with it.");
            return false; // no action taken
        }
    }
}
