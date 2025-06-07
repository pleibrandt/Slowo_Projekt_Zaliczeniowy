using System.Text.Json;

namespace Slowo_Projekt_Zaliczeniowy
{
    public partial class Form1 : Form
    {
        //odczyt
        private static readonly HttpClient client = new HttpClient();
        private static readonly HttpClient clientslow = new HttpClient();

        //liczba prob, pol, zgadniec z rzedu i punktow
        public static class Licznik
        {
            public static int LiczbaProb = 6;
            public static int Pola = 30;
            public static int ZRzedu = 0;
            public static int Punkty = 0;
        }

        public Form1()
        {
            InitializeComponent();

            LosujSlowo();
        }

        //Asynchronicznie pobiera jedno losowe piêcioliterowe s³owo z publicznego API.
        private async Task<string> PobierzLosoweSlowoAsync()
        {
            string url = "https://random-word-api.vercel.app/api?words=1&length=5";
            try
            {
                var odpowiedz = await client.GetStringAsync(url);
                var slowa = JsonSerializer.Deserialize<string[]>(odpowiedz);
                return slowa?[0] ?? "Blad";
            }
            catch (Exception ex)
            {
                return $"Blad: {ex.Message}";
            }
        }
        //Sprawdza, czy dane s³owo istnieje w angielskim s³owniku publicznego API.
        private async Task<bool> CzySlowoIstniejeAsync(string slowo)
        {
            string url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{slowo}";

            try
            {
                var odpowiedz = await clientslow.GetAsync(url);
                return odpowiedz.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show("B³¹d po³¹czenia: " + ex.Message);
                return false;
            }
        }

        //losowanie slowa
        private async void LosujSlowo()
        {
            string slowo = await PobierzLosoweSlowoAsync();
            slowo = slowo.ToUpper();
            wygrywajacy.Text = slowo;
        }

        //sprawdzanie czy w s³owie jest dana litera
        private bool CzyJestTakaLitera(char znak)
        {
            string wygrywa = wygrywajacy.Text;
            for (int i = 0; i < 5; i++)
            {
                if (wygrywa[i] == znak)
                    return true;
            }
            return false;
        }
        //sprawdzanie czy litera zgadywana jest w tym samym miejscu co litera wygrywajaca
        private bool CzyJestTakaLiteraDokladnie(char znak, int i)
        {
            string wygrywa = wygrywajacy.Text;
            if (wygrywa[i] == znak)
                return true;
            else
                return false;
        }
        //sprawdzanie tego czy ostatnie slowo jest slowem wygrywajacym
        private void SzukajZwyciezcy(string odp)
        {
            if (odp == wygrywajacy.Text)
            {
                MessageBox.Show("Uda³o ci siê odgadn¹æ s³owo!", "Brawo!");
                Licznik.Pola = 30;
                Licznik.ZRzedu++;
                LosujSlowo();
                streak.Text = Convert.ToString(Licznik.ZRzedu);
                //mechanizm obliczania punktów na podstawie szybkoœci odgadniêcia s³owa
                switch (Licznik.LiczbaProb)
                {
                    case 5:
                        Licznik.Punkty = Licznik.Punkty + 200;
                        break;
                    case 4:
                        Licznik.Punkty = Licznik.Punkty + 100;
                        break;
                    case 3:
                        Licznik.Punkty = Licznik.Punkty + 50;
                        break;
                    case 2:
                        Licznik.Punkty = Licznik.Punkty + 25;
                        break;
                    case 1:
                        Licznik.Punkty = Licznik.Punkty + 10;
                        break;
                    case 0:
                        Licznik.Punkty = Licznik.Punkty + 5;
                        break;
                }
                pkt.Text = Convert.ToString(Licznik.Punkty);
                Licznik.LiczbaProb = 6;
                for (int i = 0; i < 30; i++)
                {
                    tableLayoutPanel1.Controls[i].Text = "";
                    tableLayoutPanel1.Controls[i].BackColor = Color.DarkGray;
                }
                for (int i = 0; i < 26; i++)
                {
                    Litery.Controls[i].BackColor = Color.DarkGray;
                }
            }
        }
        //sprawdzanie czy uzytkownikowy nie skonczyly sie proby
        private void SzukajKoncaProb()
        {
            string wygrywa = wygrywajacy.Text;
            if (Licznik.LiczbaProb == 0)
            {
                MessageBox.Show("Skonczyly ci sie mozliwe proby. Poprawne slowo to " + wygrywa, "Porazka");
                Close();
            }
        }
        //sprawdzanie czy dana literka jeszcze wystêpuje w danym s³owie
        private bool CzyZostalyInne(string slowo, int a)
        {
            string wygrywa = wygrywajacy.Text;
            char wybranaLiterka = slowo[a];
            int ileWygrywa = 0, ileJest = 0;
            for (int i = 0; i < 5; i++)
            {
                if (wybranaLiterka == wygrywa[i])
                    ileJest++;
                if ((CzyJestTakaLiteraDokladnie(slowo[i], i) == true) && (slowo[i] == wybranaLiterka))
                    ileWygrywa++;
            }
            if (ileWygrywa == ileJest)
                return false;
            else
                return true;

        }
        //sprawdzanie czy s³owo istnieje w s³owniku jêzyka angielskiego dziêki wykorzystaniu darmowego API
        private async Task CzyIstniejeWSlowniku(string slowo)
        {
            bool istnieje = await CzySlowoIstniejeAsync(slowo);

            if (istnieje == false)
            {
                MessageBox.Show($"Slowo \"{slowo}\" NIE istnieje w slowniku.", "Blad");
                odpowiedz.Clear();
            }
        }

        //przysik przesylania odpowiedzi
        private async void przyciskPotwierdzenia_Click(object sender, EventArgs e)
        {
            await CzyIstniejeWSlowniku(odpowiedz.Text);
            string odp = odpowiedz.Text;
            odp = odp.ToUpper();
            if (odp.Length == 0)
                return;
            //sprawdzanie czy slowo ma 5 liter, jesli nie wyskoczy okienko z bledem
            if (odp.Length != 5)
            {
                MessageBox.Show("Podaj s³owo ktore ma 5 liter", "Blad");
                odpowiedz.Clear();
            }
            else
            {                
                char[] znak = new char[5];
                for (int i = 0; i < 5; i++)
                {
                    znak[i] = odp[i];
                }
                for (int i = 0; i < 5; i++)
                {
                    tableLayoutPanel1.Controls[Licznik.Pola - 1].Text = Convert.ToString(znak[i]);
                    if (CzyJestTakaLiteraDokladnie(znak[i], i) == true)
                    {
                        tableLayoutPanel1.Controls[Licznik.Pola - 1].BackColor = Color.LightGreen;
                        Litery.Controls[25 - (Convert.ToInt32(znak[i]) - 65)].BackColor = Color.LightGreen;
                    }
                    else if ((CzyJestTakaLitera(znak[i]) == true) && (CzyZostalyInne(odp, i) == true))
                    {
                        tableLayoutPanel1.Controls[Licznik.Pola - 1].BackColor = Color.Khaki;
                        if (Litery.Controls[25 - (Convert.ToInt32(znak[i]) - 65)].BackColor != Color.LightGreen)
                            Litery.Controls[25 - (Convert.ToInt32(znak[i]) - 65)].BackColor = Color.Khaki;
                    }
                    else
                    {
                        tableLayoutPanel1.Controls[Licznik.Pola - 1].BackColor = Color.IndianRed;
                        if ((Litery.Controls[25 - (Convert.ToInt32(znak[i]) - 65)].BackColor != Color.LightGreen) && (Litery.Controls[25 - (Convert.ToInt32(znak[i]) - 65)].BackColor != Color.Khaki))
                            Litery.Controls[25 - (Convert.ToInt32(znak[i]) - 65)].BackColor = Color.IndianRed;
                    }
                    Licznik.Pola--;
                }
                Licznik.LiczbaProb--;
                SzukajZwyciezcy(odp);
                SzukajKoncaProb();
                odpowiedz.Clear();
            }
        }
        //pobieranie slowa z API przy starcie
        private async void Form1_Load(object sender, EventArgs e)
        {
            string word = await PobierzLosoweSlowoAsync();
            word = word.ToUpper();
            wygrywajacy.Text = word;
            odpowiedz.Focus();
        }
        private void label32_Click(object sender, EventArgs e)
        {
            Label? clickedLabel = sender as Label;
            if(clickedLabel != null)
            {
                if(odpowiedz.Text.Length<5)
                    odpowiedz.Text = odpowiedz.Text + clickedLabel.Text;
            }
        }
    }
}
