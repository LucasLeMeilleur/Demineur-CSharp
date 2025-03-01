
using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
using System.Threading;
class MainProg
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
            if(difficulty >= 0 && difficulty <= 2)
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

        while (!demineur.IsExploded()){
            Console.WriteLine();
            Console.Write("Quel est votre commande : ");
            string Commande = Console.ReadLine();
            if (string.IsNullOrEmpty(Commande)) Commande = "";
          
            if (Commande.Length == 3){
                Thread.Sleep(1200);
                Console.Clear();
                demineur.Commande(Commande);
                Console.WriteLine("Nombre de bombes restantes : " + demineur.NbDeBombe());
                demineur.AfficherTableauHide();
                demineur.AfficherTableauShow();
                demineur.ListerCmd();
            }
            else {
                Console.WriteLine("Erreur de commande");
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine("Nombre de bombes restantes : " + demineur.NbDeBombe());
                demineur.AfficherTableauHide();
                demineur.AfficherTableauShow();
                demineur.ListerCmd();
            }
            if (demineur.Fini()){
                demineur.RevelerLesBombes();
                break;
            }
        }
        if (demineur.IsExploded()){
            Console.Clear();
            demineur.RevelerLesBombes();
            demineur.AfficherTableauShow();
            Console.WriteLine("Perdu !");
        }
        else{
            Console.Clear();
            demineur.AfficherTableauShow();
            Console.WriteLine("Gagné !");
        }
    }
}

class Bombe
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
        else if(difficulty == 2)
        {
            TailleXTableau = 16; //Nombre de lignes
            TailleYTableau = 32; //Nombre de colonnes
            NbBombeTot = 99;
        }
        NbBombeRestante = NbBombeTot;
    }
    public void InitialiserJeu(int PosDepartX=4, int PosDepartY=3){

        TableauJeuHide = new int[TailleXTableau, TailleYTableau];
        TableauJeuShow = new char[TailleXTableau, TailleYTableau];
        int nbBombe = NbBombeTot;

        for (int i = 0; i < TailleXTableau; i++) for (int j = 0; j < TailleYTableau; j++) TableauJeuHide[i, j] = 0;

        for (int i = 0; i < TailleXTableau; i++) for (int j = 0; j < TailleYTableau; j++) TableauJeuShow[i, j] = 'X';


        Random rand = new Random();

        while (nbBombe > 0){
            int ligne = rand.Next(TailleXTableau);
            int colonne = rand.Next(TailleYTableau);
            bool ConditionPos = VerifierCondition(ligne, colonne, PosDepartX, PosDepartY);
            if (TableauJeuHide[ligne, colonne] != 10 && ConditionPos){
                TableauJeuHide[ligne, colonne] = 10;
                nbBombe--;
            }
        }

        //Parcour la map pour definir le nb de bombe autour

        for (int i = 0; i < TailleXTableau; i++){
            for (int j = 0; j < TailleYTableau; j++){
                if (TableauJeuHide[i,j] == 10){
                    if (i == 0)
                    {
                        if (j != 0){
                            AjouterTab(ref TableauJeuHide, i, j - 1);
                            AjouterTab(ref TableauJeuHide, i+1, j - 1);
                        }
                        if (j != (TailleYTableau-1)){
                            AjouterTab(ref TableauJeuHide, i, j + 1);
                            AjouterTab(ref TableauJeuHide, i + 1, j + 1);
                        }
                        AjouterTab(ref TableauJeuHide, i+1, j );
                    }
                    else if (i == (TailleXTableau-1)){
                        if (j != 0)
                        {
                            AjouterTab(ref TableauJeuHide, i, j - 1);
                            AjouterTab(ref TableauJeuHide, i - 1, j - 1);
                        }
                        if (j != (TailleYTableau-1))
                        {
                            AjouterTab(ref TableauJeuHide, i, j + 1);
                            AjouterTab(ref TableauJeuHide, i - 1, j + 1);
                        }
                        AjouterTab(ref TableauJeuHide, i - 1, j);
                    }
                    else
                    {
                        if (j != 0)
                        {
                            AjouterTab(ref TableauJeuHide, i, j - 1);
                            AjouterTab(ref TableauJeuHide, i - 1, j - 1);
                            AjouterTab(ref TableauJeuHide, i + 1, j - 1);
                        }
                        if (j != (TailleYTableau-1))
                        {
                            AjouterTab(ref TableauJeuHide, i, j + 1);
                            AjouterTab(ref TableauJeuHide, i - 1, j + 1);
                            AjouterTab(ref TableauJeuHide, i + 1, j + 1);
                        }
                        AjouterTab(ref TableauJeuHide, i - 1, j);
                        AjouterTab(ref TableauJeuHide, i + 1, j);

                    }
                }
            }
        }

    }
    public void AfficherTableauHide()
    {
        for (int i = 0; i < TailleXTableau; i++)
        {
            for (int j = 0; j < TailleYTableau; j++)
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
        for (int i = 0; i < TailleXTableau; i++)
        {
            for (int j = 0; j < TailleYTableau; j++)
            {
                if (TableauJeuHide[i, j] == 'B') Console.Write(TableauJeuShow[i, j] + " ");
                else Console.Write(TableauJeuShow[i, j] + "  ");

            }
            Console.WriteLine();
        }
    }
    private void AjouterTab(ref int[,] tableau, int a, int b)
    {
        tableau[a,b] = tableau[a, b] != 10 ? tableau[a, b] + 1 : tableau[a, b];
    }
    private bool VerifierCondition(int ligne, int colonne, int choixX, int choixY)
    {
        bool Condition = true;

        if (ligne == choixX || ligne == choixX - 1 || ligne == choixX + 1) Condition = false;
        if (colonne == choixY || colonne == choixY - 1 || colonne == choixY + 1) Condition = false;
        if (
            ( (colonne == (choixY + 1)) && (ligne == (choixX + 1)) ) || 
            ( (colonne == (choixY - 1)) && (ligne == (choixX + 1)) ) ||
            ( (colonne == (choixY + 1)) && (ligne == (choixX - 1)) ) || 
            ( (colonne == (choixY - 1)) && (ligne == (choixX - 1)) )
            )
        {

            Condition = false;
        }
        return Condition;
    }
    public void DevoilerLesCases()
    {
        for (int i = 0; i < TailleXTableau; i++)
        {
            for (int j = 0; j < TailleYTableau; j++)
            {
                if (TableauJeuHide[i, j] == 0)
                {
                    TableauJeuShow[i, j] = (char)('0' + TableauJeuHide[i, j]);
                    if (j != 0)
                    {
                        TableauJeuShow[i, j-1] = (char)('0' + TableauJeuHide[i, j - 1]);
                    }
                    if (j != (TailleYTableau-1))
                    {
                        TableauJeuShow[i, j + 1] = (char)('0'+TableauJeuHide[i, j + 1]);
                    }
                    if(i != 0)
                    {
                        TableauJeuShow[i-1, j] = (char)('0' + TableauJeuHide[i-1,j]);
                    }
                    if (i!= (TailleXTableau-1))   
                    {
                        TableauJeuShow[i+1, j] = (char)('0'+ TableauJeuHide[i+1, j]);
                    }
                    if (i != 0 && j!= (TailleYTableau-1))
                    {
                        TableauJeuShow[i - 1, j+ 1] = (char)('0' + TableauJeuHide[i - 1, j+1]);

                    }

                    if (i != (TailleXTableau-1) && j != (TailleYTableau-1))
                    {
                        TableauJeuShow[i + 1, j + 1] = (char)('0' + TableauJeuHide[i + 1, j + 1]);

                    }

                    if (i != (TailleXTableau-1) && j != 0)
                    {
                        TableauJeuShow[i + 1, j - 1] = (char)('0' + TableauJeuHide[i + 1, j - 1]);

                    }
                    if (i != 0 && j != 0)
                    {
                        TableauJeuShow[i - 1, j - 1] = (char)('0' + TableauJeuHide[i - 1, j - 1]);

                    }
                }
            }
        }
    }
    private void Flag(int PosX, int PosY)
    {
        TableauJeuShow[PosX, PosY] = 'F';
        CheckBombeRestante();
    }
    private void Question(int PosX, int PosY)
    {
        TableauJeuShow[PosX, PosY] = 'Q';
    }
    private void RetirerSupposition(int PosX, int PosY)
    {
        TableauJeuShow[PosX, PosY] = 'X';
    }
    private void CheckBombeRestante()
    {
        NbBombeRestante = NbBombeTot;
        for (int i = 0; i < TailleXTableau; i++)
        {
            for (int j = 0; j < TailleYTableau; j++)
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
        int PosY = Cmd[2]- '0';

        if (PosX > 7) return;
        if (PosY > 9) return;

        if (Char.IsDigit(TableauJeuShow[PosX, PosY])) return;

        if (Instruction == 'F')
        {
            Flag(PosX, PosY);
        }
        else if(Instruction == 'Q')
        {
            Question(PosX, PosY);
        }
        else if (Instruction == 'X')
        {
            RetirerSupposition(PosX, PosY);
        }
        else if(Instruction == 'D')
        {
            DecouvrirUneCase(PosX, PosY);
        }
    }
    private void DecouvrirUneCase(int PosX, int PosY)
    {
        if (TableauJeuHide[PosX, PosY] == 10){
            Exploded = true;
        }
        else{
            TableauJeuShow[PosX, PosY] = (char)('0' + TableauJeuHide[PosX, PosY]);
        }
    }
    public void RevelerLesBombes(){
        for(int i = 0; i < TailleXTableau; i++) for(int j = 0; j < TailleYTableau; j++){
                if (TableauJeuHide[i,j] == 10) TableauJeuShow[i, j] = 'B';
            }
    }
    public int NbDeBombe() { return NbBombeRestante; }

    public bool Fini(){
        int NbdeCaseDecouverte = 0;
        int NombreTotaleDeCase = (TailleYTableau * TailleXTableau) - NbBombeTot;
        for(int i = 0; i < TailleXTableau; i++) for (int j = 0; j < TailleYTableau; j++){
            if (Char.IsDigit(TableauJeuShow[i, j])) NbdeCaseDecouverte++;
        }
        if (NbdeCaseDecouverte == NombreTotaleDeCase) return true;
        else return false;
    }
 }