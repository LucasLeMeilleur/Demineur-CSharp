
using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
using System.Threading;

using System.Diagnostics;


namespace Demineur
{
    public class Demineur
    {
        static void Main(string[] args)
        {

            int difficulty = 0;
            while (true)
            {
                Console.WriteLine("Choisissez votre diffiuculté (0-2) : ");
                try
                {
                    difficulty = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.WriteLine("Format invalide, 1 entier est requis");
                    Thread.Sleep(1000);
                    Console.Clear();
                }
                if (difficulty >= 0 && difficulty <= 2)
                {
                    Console.WriteLine("Difficulté choisi !");
                    Thread.Sleep(1000);
                    break;
                }
                else
                {
                    Console.WriteLine("Difficulté non comprise, veuillez choisir entre 0 et 2");
                    Console.Clear();
                }
            }

            Bombe demineur = new Bombe(difficulty);

            Console.Clear();



            demineur.InitialiserJeu();



            Console.WriteLine("Nombre de bombes restantes : " + demineur.NbDeBombe());

            demineur.AfficherTableauHide();
            demineur.DevoilerLesCases();
            demineur.AfficherTableauShow();
            demineur.ListerCmd();

            while (!demineur.IsExploded())
            {
                Console.WriteLine();
                Console.Write("Quel est votre commande : ");
                string Commande = Console.ReadLine();
                if (string.IsNullOrEmpty(Commande)) Commande = "";

                if (Commande.Length == 3)
                {
                    Thread.Sleep(1200);
                    Console.Clear();
                    demineur.Commande(Commande);
                    Console.WriteLine("Nombre de bombes restantes : " + demineur.NbDeBombe());
                    demineur.AfficherTableauHide();
                    demineur.AfficherTableauShow();
                    demineur.ListerCmd();
                }
                else
                {
                    Console.WriteLine("Erreur de commande");
                    Thread.Sleep(1000);
                    Console.Clear();
                    Console.WriteLine("Nombre de bombes restantes : " + demineur.NbDeBombe());
                    demineur.AfficherTableauHide();
                    demineur.AfficherTableauShow();
                    demineur.ListerCmd();
                }
                if (demineur.Fini())
                {
                    demineur.RevelerLesBombes();
                    break;
                }
            }
            if (demineur.IsExploded())
            {
                Console.Clear();
                demineur.RevelerLesBombes();
                demineur.AfficherTableauShow();
                Console.WriteLine("Perdu !");
            }
            else
            {
                Console.Clear();
                demineur.AfficherTableauShow();
                Console.WriteLine("Gagné !");
            }
        }
    }

    public class Bombe
    {
        private static int[,] TableauJeuHide = { { 0, 0 } };
        private static char[,] TableauJeuShow = { { 'X', 'X' } };
        private static int NbBombeRestante;
        private static bool Exploded = false;
        private static int TailleXTableau, TailleYTableau, NbBombeTot;
        public Bombe(int difficulty)
        {
            if (difficulty == 0)
            {
                TailleXTableau = 8;
                TailleYTableau = 8;
                NbBombeTot = 10;
            }
            else if (difficulty == 1)
            {
                TailleXTableau = 16;
                TailleYTableau = 16;
                NbBombeTot = 40;
            }
            else if (difficulty == 2)
            {
                TailleXTableau = 32; //Nombre de lignes
                TailleYTableau = 16; //Nombre de colonnes
                NbBombeTot = 99;
            }
            NbBombeRestante = NbBombeTot;

            TableauJeuHide = new int[TailleYTableau, TailleXTableau];
            TableauJeuShow = new char[TailleYTableau, TailleXTableau];
        }
        public void InitialiserJeu(int PosDepartX = 4, int PosDepartY = 3)
        {

            Debug.WriteLine("Jeu initialisé");

            int nbBombe = NbBombeTot;

            for (int i = 0; i < TailleYTableau; i++) for (int j = 0; j < TailleXTableau; j++) TableauJeuHide[i, j] = 0;

            for (int i = 0; i < TailleYTableau; i++) for (int j = 0; j < TailleXTableau; j++) TableauJeuShow[i, j] = 'X';


            Random rand = new Random();

            while (nbBombe > 0)
            {
                int ligne = rand.Next(TailleYTableau);
                int colonne = rand.Next(TailleXTableau);
                bool ConditionPos = VerifierCondition(ligne, colonne, PosDepartX, PosDepartY);
                if (TableauJeuHide[ligne, colonne] != 10 && ConditionPos)
                {
                    TableauJeuHide[ligne, colonne] = 10;
                    nbBombe--;
                }
            }

            //Parcour la map pour definir le nb de bombe autour
            for (int j = 0; j < TailleYTableau; j++) // Inversé X et Y
            {
                for (int i = 0; i < TailleXTableau; i++)
                {
                    if (TableauJeuHide[j, i] == 10) // Inversion ici aussi
                    {
                        if (j == 0)
                        {
                            if (i != 0)
                            {
                                AjouterTab(ref TableauJeuHide, j, i - 1);
                                AjouterTab(ref TableauJeuHide, j + 1, i - 1);
                            }
                            if (i != (TailleXTableau - 1))
                            {
                                AjouterTab(ref TableauJeuHide, j, i + 1);
                                AjouterTab(ref TableauJeuHide, j + 1, i + 1);
                            }
                            AjouterTab(ref TableauJeuHide, j + 1, i);
                        }
                        else if (j == (TailleYTableau - 1))
                        {
                            if (i != 0)
                            {
                                AjouterTab(ref TableauJeuHide, j, i - 1);
                                AjouterTab(ref TableauJeuHide, j - 1, i - 1);
                            }
                            if (i != (TailleXTableau - 1))
                            {
                                AjouterTab(ref TableauJeuHide, j, i + 1);
                                AjouterTab(ref TableauJeuHide, j - 1, i + 1);
                            }
                            AjouterTab(ref TableauJeuHide, j - 1, i);
                        }
                        else
                        {
                            if (i != 0)
                            {
                                AjouterTab(ref TableauJeuHide, j, i - 1);
                                AjouterTab(ref TableauJeuHide, j - 1, i - 1);
                                AjouterTab(ref TableauJeuHide, j + 1, i - 1);
                            }
                            if (i != (TailleXTableau - 1))
                            {
                                AjouterTab(ref TableauJeuHide, j, i + 1);
                                AjouterTab(ref TableauJeuHide, j - 1, i + 1);
                                AjouterTab(ref TableauJeuHide, j + 1, i + 1);
                            }
                            AjouterTab(ref TableauJeuHide, j - 1, i);
                            AjouterTab(ref TableauJeuHide, j + 1, i);
                        }
                    }
                }
            }
        }
        public void AfficherTableauHide()
        {
            for (int i = 0; i < TailleYTableau; i++)
            {
                for (int j = 0; j < TailleXTableau; j++)
                {
                    if (TableauJeuHide[i, j] == 10) Console.Write(TableauJeuHide[i, j] + " ");
                    else Console.Write(TableauJeuHide[i, j] + "  ");

                }
                Console.WriteLine();
            }
        }
        public void ListerCmd()
        {
            Console.WriteLine();
            Console.WriteLine("F.. pour mettre un flag");
            Console.WriteLine("D.. pour découvrir une case");
            Console.WriteLine("X.. pour retirer une supposition");
            Console.WriteLine("Q.. pour supposer une case");
        }
        public void AfficherTableauShow()
        {
            Console.WriteLine("");
            for (int i = 0; i < TailleYTableau; i++)
            {
                for (int j = 0; j < TailleXTableau; j++)
                {
                    if (TableauJeuHide[i, j] == 'B') Console.Write(TableauJeuShow[i, j] + " ");
                    else Console.Write(TableauJeuShow[i, j] + "  ");

                }
                Console.WriteLine();
            }
        }
        private void AjouterTab(ref int[,] tableau, int a, int b)
        {
            tableau[a, b] = tableau[a, b] != 10 ? tableau[a, b] + 1 : tableau[a, b];
        }
        private bool VerifierCondition(int ligne, int colonne, int choixX, int choixY)
        {
            bool Condition = true;

            if (ligne == choixX || ligne == choixX - 1 || ligne == choixX + 1) Condition = false;
            if (colonne == choixY || colonne == choixY - 1 || colonne == choixY + 1) Condition = false;
            if (
                ((colonne == (choixY + 1)) && (ligne == (choixX + 1))) ||
                ((colonne == (choixY - 1)) && (ligne == (choixX + 1))) ||
                ((colonne == (choixY + 1)) && (ligne == (choixX - 1))) ||
                ((colonne == (choixY - 1)) && (ligne == (choixX - 1)))
                )
            {

                Condition = false;
            }
            return Condition;
        }
        public void DevoilerLesCases()
        {
            for (int j = 0; j < TailleYTableau; j++) // On inverse les boucles
            {
                for (int i = 0; i < TailleXTableau; i++)
                {
                    if (TableauJeuHide[j, i] == 0) // On inverse aussi les accès au tableau
                    {
                        TableauJeuShow[j, i] = (char)('0' + TableauJeuHide[j, i]);

                        if (i != 0)
                        {
                            TableauJeuShow[j, i - 1] = (char)('0' + TableauJeuHide[j, i - 1]);
                        }
                        if (i != (TailleXTableau - 1))
                        {
                            TableauJeuShow[j, i + 1] = (char)('0' + TableauJeuHide[j, i + 1]);
                        }
                        if (j != 0)
                        {
                            TableauJeuShow[j - 1, i] = (char)('0' + TableauJeuHide[j - 1, i]);
                        }
                        if (j != (TailleYTableau - 1))
                        {
                            TableauJeuShow[j + 1, i] = (char)('0' + TableauJeuHide[j + 1, i]);
                        }
                        if (j != 0 && i != (TailleXTableau - 1))
                        {
                            TableauJeuShow[j - 1, i + 1] = (char)('0' + TableauJeuHide[j - 1, i + 1]);
                        }
                        if (j != (TailleYTableau - 1) && i != (TailleXTableau - 1))
                        {
                            TableauJeuShow[j + 1, i + 1] = (char)('0' + TableauJeuHide[j + 1, i + 1]);
                        }
                        if (j != (TailleYTableau - 1) && i != 0)
                        {
                            TableauJeuShow[j + 1, i - 1] = (char)('0' + TableauJeuHide[j + 1, i - 1]);
                        }
                        if (j != 0 && i != 0)
                        {
                            TableauJeuShow[j - 1, i - 1] = (char)('0' + TableauJeuHide[j - 1, i - 1]);
                        }
                    }
                }
            }
        }

        public void Flag(int PosY, int PosX)
        {
            TableauJeuShow[PosY, PosX] = 'F';
            CheckBombeRestante();
        }
        public void Question(int PosY, int PosX)
        {
            TableauJeuShow[PosY, PosX] = 'Q';
        }
        public void RetirerSupposition(int PosY, int PosX)
        {
            TableauJeuShow[PosY, PosX] = 'X';
        }
        private void CheckBombeRestante()
        {
            NbBombeRestante = NbBombeTot;
            for (int i = 0; i < TailleYTableau; i++)
            {
                for (int j = 0; j < TailleXTableau; j++)
                {
                    if (TableauJeuShow[i, j] == 'F') NbBombeRestante--;
                }
            }
        }
        public bool IsExploded()
        {
            return Exploded;
        }
        public void Commande(string Cmd)
        {
            char Instruction = Cmd[0];
            int PosX = Cmd[1] - '0';
            int PosY = Cmd[2] - '0';

            if (PosX > 7) return;
            if (PosY > 9) return;

            if (Char.IsDigit(TableauJeuShow[PosY, PosX])) return;

            if (Instruction == 'F')
            {
                Flag(PosX, PosY);
            }
            else if (Instruction == 'Q')
            {
                Question(PosX, PosY);
            }
            else if (Instruction == 'X')
            {
                RetirerSupposition(PosX, PosY);
            }
            else if (Instruction == 'D')
            {
                DecouvrirUneCase(PosX, PosY);
            }
        }
        public void DecouvrirUneCase(int PosY, int PosX)
        {
            if (TableauJeuHide[PosY, PosX] == 10)
            {
                Exploded = true;
            }
            else
            {
                TableauJeuShow[PosY, PosX] = (char)('0' + TableauJeuHide[PosY, PosX]);
            }
        }
        public void RevelerLesBombes()
        {
            for (int i = 0; i < TailleYTableau; i++) for (int j = 0; j < TailleXTableau; j++)
                {
                    if (TableauJeuHide[i, j] == 10) TableauJeuShow[i, j] = 'B';
                }
        }
        public int NbDeBombe() { return NbBombeRestante; }
        public int LargeurTableau() { return TailleXTableau;  }
        public int HauteurTableau() { return TailleYTableau;  }
        public int DonnerTableauHide(int PosY, int PosX) { return TableauJeuHide[PosY, PosX]; }

        public int DonnerTableauShow(int PosY, int PosX) { return TableauJeuShow[PosY, PosX]; }

        public void TableauShow()
        {
            for (int i = 0; i < TailleYTableau; i++)
            {
                for (int j = 0; j < TailleXTableau; j++)
                {
                    Debug.Write(TableauJeuShow[i, j]);
                }
                Debug.WriteLine("");
            }
        }

        public bool ExisteUneTuile(int PosX, int PosY)
        {
            if (TableauJeuShow[PosY, PosX] == 'X')
            {
                return true;
            }
            else return false;
        }
        public bool Fini()
        {
            int NbdeCaseDecouverte = 0;
            int NombreTotaleDeCase = (TailleYTableau * TailleXTableau) - NbBombeTot;
            for (int i = 0; i < TailleYTableau; i++) for (int j = 0; j < TailleXTableau; j++)
                {
                    if (Char.IsDigit(TableauJeuShow[i, j])) NbdeCaseDecouverte++;
                }
            if (NbdeCaseDecouverte == NombreTotaleDeCase) return true;
            else return false;
        }

        public void VoirLenghtTableau()
        {
            Debug.WriteLine("Taille");
            Debug.WriteLine(TableauJeuShow.GetLength(0));
            Debug.WriteLine(TableauJeuShow.GetLength(1));
            return;
        }

        public void ResetTableauShow()
        {

            for (int i = 0; i < TailleYTableau; i++) for (int j = 0; j < TailleXTableau; j++) TableauJeuShow[i, j] = 'X';
        }
    }
}
