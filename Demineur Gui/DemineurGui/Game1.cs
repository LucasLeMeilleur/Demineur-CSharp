using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Demineur;
using SharpDX.Direct2D1;
using MonoSpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;
using SharpDX.Direct2D1.Effects;
using SharpDX.Direct3D9;
using System.Collections.Generic;
using System;
using SharpDX.MediaFoundation;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace DemineurGui
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        // Constructeur pour initialiser x et y
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        // Méthode pour afficher les coordonnées
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
    public class Game1 : Game
    {

        private Texture2D tileTexture;
        private Texture2D chiffresTexture;
        private Texture2D visageTexture;
        private Texture2D niveauTexture;
        private Texture2D rectTexture;

        private Texture2D premierCadre;
        private Texture2D deuxiemeCadre;
        private GraphicsDeviceManager graphics;
        private Vector2 tilePosition;
        private Vector2 chiffresPosition;
        private Vector2 visagePosition;
        private Vector2 niveauPosition;
        private MonoSpriteBatch spriteTuile;
        private int Difficulte= 0;

        private Bombe demineur;
        private MouseState mouseState = Mouse.GetState();
        private bool isClicked = false;
        private List<Rectangle> tileRectangles = new List<Rectangle>();
        private bool PremierClick = false;
        private bool JeuInitialise = false;
        private MouseState previousMouseState;
        private int compteur =0;
        private Thread threadCompteur;
        private bool Perdu = false;
        private bool ClicDroit = false;
        private int[] tuileMaintenu = new int[2];
        private Rectangle destinationRectangleVisage;
        private Rectangle destinationRectangleDifficulte;
        private bool VisageClique = false;
        private CancellationTokenSource cts;
        private bool PartieDemarre = false;
        private bool DifficulteClique=false;
        private bool JeuTermine = false;
        private int LargeurFenetre = 900, HauteurFenetre = 600;
        private int OffsetRegleX = 0, OffsetRegleY = 0;


        public Game1()
        {
            demineur = new Bombe(Difficulte);
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = LargeurFenetre; // Largeur
            graphics.PreferredBackBufferHeight = HauteurFenetre; // Hauteur
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            threadCompteur = new Thread(() => Compteur(cts.Token)); 
            tilePosition = new Vector2(graphics.PreferredBackBufferWidth / 2,
                                       graphics.PreferredBackBufferHeight / 2);
            chiffresPosition = new Vector2(graphics.PreferredBackBufferWidth / 2,
                                   graphics.PreferredBackBufferHeight / 2);
            visagePosition = new Vector2(graphics.PreferredBackBufferWidth / 2,
                                   graphics.PreferredBackBufferHeight / 2);
            niveauPosition = new Vector2(graphics.PreferredBackBufferWidth / 2,
                                       graphics.PreferredBackBufferHeight / 2);
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            int scale = 5;
            int tileSize = 128 / scale;
            int offsetX = OffsetRegleX, offsetY = OffsetRegleY; 
            int tuileX, tuileY;
            if( ( mouseState.LeftButton == ButtonState.Pressed || previousMouseState.LeftButton == ButtonState.Pressed ) && destinationRectangleVisage.Contains(mouseState.Position) )
            {
                VisageClique = true;
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    if (threadCompteur.IsAlive)
                    {
                        cts.Cancel();
                        threadCompteur.Join();
                    }
                    compteur = 0;
                    tileRectangles.Clear();
                    JeuInitialise = false;
                    PremierClick = false;
                    Perdu = false;
                    demineur = null;
                    JeuTermine = false;
                    demineur = new Bombe(Difficulte);
                    PartieDemarre = false;
                    demineur.ResetTableauShow();
                }
            }
            else
            {
                VisageClique = false;
            }

            if ((mouseState.LeftButton == ButtonState.Pressed || previousMouseState.LeftButton == ButtonState.Pressed) && destinationRectangleDifficulte.Contains(mouseState.Position))
            {
                DifficulteClique = true;
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    if (threadCompteur.IsAlive)
                    {
                        cts.Cancel();
                        threadCompteur.Join();
                    }
                    
                    Difficulte += 1;
                    Difficulte = Difficulte % 3;
                    tileRectangles.Clear();
                    compteur = 0;
                    JeuInitialise = false;
                    Perdu = false;
                    JeuTermine = false;
                    PremierClick = false;
                    demineur = null;
                    demineur = new Bombe(Difficulte);
                    PartieDemarre = false;
                    demineur.ResetTableauShow();
                }
            }
            else
            {
                DifficulteClique = false;
            }


            if (previousMouseState.LeftButton == ButtonState.Pressed || mouseState.LeftButton == ButtonState.Pressed )
            { 
                tuileX = (mouseState.X - offsetX) / tileSize;
                tuileY = (mouseState.Y - offsetY) / tileSize;
                // Vérifier si les indices sont valides
                if (tuileX >= 0 && tuileX < demineur.LargeurTableau() &&
                        tuileY >= 0 && tuileY < demineur.HauteurTableau())
                    {

                    tuileMaintenu[0] = tuileX;
                    tuileMaintenu[1] = tuileY;
                    ClicDroit = true;

                    if (mouseState.LeftButton == ButtonState.Released)
                    {
                        if (!PremierClick)
                        {
                            if (threadCompteur.IsAlive)
                            {
                                cts.Cancel();
                                threadCompteur.Join();
                                tileRectangles.Clear();
                            }

                            tileRectangles.Clear();
                            demineur.InitialiserJeu(tuileX, tuileY);
                            demineur.DevoilerLesCases(tuileX, tuileY);
                            PremierClick = true;
                            JeuInitialise = true;
                            compteur = 0;
                            cts = new CancellationTokenSource();
                            threadCompteur = new Thread(() => Compteur(cts.Token));
                            threadCompteur.Start();
                            PartieDemarre = true;
                        }
                        else
                        {
                            if (!Perdu)
                            {
                                demineur.DecouvrirUneCase(tuileY, tuileX);
                            }

                            if (demineur.DonnerTableauHide(tuileY, tuileX) == 10)
                            {
                                demineur.RevelerLesBombes();
                                Perdu = true;
                            }
                            if(demineur.DonnerTableauHide(tuileY, tuileX) == 0)
                            {
                                demineur.DevoilerLesCases(tuileX, tuileY);
                            }

                        }

                    }
                }
            }
            else
            {
                ClicDroit = false;
                tuileMaintenu[0] = 10000;
                tuileMaintenu[1] = 10000;
            }

            if ( (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released) && PremierClick)
            {
                tuileX = (mouseState.X - offsetX) / tileSize;
                tuileY = (mouseState.Y - offsetY) / tileSize;
                if (tuileX >= 0 && tuileX < demineur.LargeurTableau() &&
                    tuileY >= 0 && tuileY < demineur.HauteurTableau())
                {
                    if (demineur.DonnerTableauShow(tuileY, tuileX) == 'X')
                    {
                        demineur.Flag(tuileY, tuileX);
                    }
                    else if (demineur.DonnerTableauShow(tuileY, tuileX) == 'F')
                    {
                        demineur.RetirerSupposition(tuileY, tuileX);
                    }
                }
            }
            previousMouseState = mouseState;

            if (PartieDemarre)
            {
                if (Perdu)
                {
                    cts.Cancel();
                    threadCompteur.Join();
                }
            }

            if (demineur.Fini())
            {
                cts.Cancel();
                threadCompteur.Join();
                JeuTermine = true;
            }

            base.Update(gameTime);


        }

        protected override void LoadContent()
        {
            spriteTuile = new MonoSpriteBatch(GraphicsDevice);

            tileTexture = Content.Load<Texture2D>("Tuiles");
            chiffresTexture = Content.Load<Texture2D>("Chiffres");
            visageTexture = Content.Load<Texture2D>("Visages");
            niveauTexture = Content.Load<Texture2D>("Niveaux");

            rectTexture = new Texture2D(GraphicsDevice, 1, 1);
            rectTexture.SetData(new Color[] { new Color(189,189,189) });


            premierCadre = CreateGradientTexture(GraphicsDevice, 200, 100, Color.White, new Color(20, 20, 20));
            deuxiemeCadre = CreateGradientTexture(GraphicsDevice, 200, 100, new Color(20, 20, 20), Color.White);
            // TODO: use this.Content to load your game content here
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(136, 136, 136));

            int[] valeurs = { 8, 17, 17 };
            int[] valeurs2 = { 8, 17, 34 };
            int LargeurTuiles = 23 * (valeurs2[Difficulte]);
            int HauteurTuiles = 23 * (valeurs[Difficulte]);

            int scale = 5;
            int NumTuile = 0;
            int Decalage = 5;
            int DecalageInterieur = 20;
            int DecalageJeu = 20;
            int HauteurMenu = 65;
            int Ecart = 15;
            int OffsetElem = 5;

            int TailleGrandCadrantX = LargeurTuiles + DecalageInterieur*2 + DecalageJeu*2 + Decalage * 4, TailleGrandCadrantY = HauteurTuiles + HauteurMenu + Decalage * 4 + Ecart + DecalageJeu * 2 + DecalageInterieur * 2;
            int HautMenuJeu = TailleGrandCadrantY - HauteurMenu - DecalageInterieur*2 - Ecart - Decalage;

            int PositionYElemMenu = (HauteurFenetre / 2) - (TailleGrandCadrantY / 2) + DecalageInterieur + 65/3;
            int PositionXElemMenu = (LargeurFenetre / 2) - (TailleGrandCadrantX / 2) + DecalageInterieur + TailleGrandCadrantX/2;

            Point positionGrandCadrant = new Point((LargeurFenetre / 2) - (TailleGrandCadrantX / 2), (HauteurFenetre / 2) - (TailleGrandCadrantY / 2));

            Point positionMenuHaut = new Point((LargeurFenetre / 2) - (TailleGrandCadrantX / 2)+DecalageInterieur, (HauteurFenetre / 2) - (TailleGrandCadrantY / 2)+DecalageInterieur);
            Point positionMenuBas = new Point((LargeurFenetre/2) - (TailleGrandCadrantX / 2) + DecalageInterieur, (HauteurFenetre / 2) - (TailleGrandCadrantY / 2) + DecalageInterieur + HauteurMenu + Ecart);
            Point positionJeux = new Point(25, 25);
            Point positionTuile = new Point((LargeurFenetre / 2) - (TailleGrandCadrantX / 2) + DecalageInterieur + DecalageJeu + Decalage, (HauteurFenetre / 2) - (TailleGrandCadrantY / 2) + DecalageInterieur + HauteurMenu + Ecart + DecalageJeu);
            Point positionBombes = new Point(PositionXElemMenu - 110 + OffsetElem, PositionYElemMenu);
            Point positionTemps = new Point(PositionXElemMenu + 20 + OffsetElem, PositionYElemMenu);
            Point positionVisage = new Point(PositionXElemMenu - 60 + OffsetElem, PositionYElemMenu-2);
            Point positionDifficulte = new Point(PositionXElemMenu - 20+ OffsetElem, PositionYElemMenu-2);

            OffsetRegleX = positionTuile.X;
            OffsetRegleY = positionTuile.Y;

            spriteTuile.Begin();

            spriteTuile.Draw(premierCadre, new Rectangle(positionGrandCadrant.X, positionGrandCadrant.Y, TailleGrandCadrantX, TailleGrandCadrantY), Color.White);
            spriteTuile.Draw(rectTexture, new Rectangle(positionGrandCadrant.X+Decalage, positionGrandCadrant.Y+Decalage, TailleGrandCadrantX - (Decalage*2), TailleGrandCadrantY - (Decalage*2)), Color.White);

            spriteTuile.Draw(deuxiemeCadre, new Rectangle(positionMenuHaut.X, positionMenuHaut.Y, TailleGrandCadrantX - (DecalageInterieur*2), HauteurMenu), Color.White);
            spriteTuile.Draw(rectTexture, new Rectangle(positionMenuHaut.X+Decalage, positionMenuHaut.Y+Decalage, TailleGrandCadrantX - (DecalageInterieur * 2)-(Decalage*2), HauteurMenu-(Decalage * 2)), Color.White);

            spriteTuile.Draw(deuxiemeCadre, new Rectangle(positionMenuBas.X, positionMenuBas.Y, TailleGrandCadrantX - (DecalageInterieur * 2), HautMenuJeu), Color.White);
            spriteTuile.Draw(rectTexture, new Rectangle(positionMenuBas.X+Decalage, positionMenuBas.Y+Decalage, TailleGrandCadrantX - (DecalageInterieur * 2) - (Decalage * 2), HautMenuJeu - (Decalage * 2)), Color.White);


            tileRectangles.Clear();

            for (int i = 0; i < demineur.LargeurTableau(); i++)
            {
                for (int j = 0; j < demineur.HauteurTableau(); j++)
                {
                    if (JeuInitialise)
                    {
                        int nombre = 0;
                        /////////////////////////// Dessiner Tableau Hide ///////////////////////////
                        
                        nombre = demineur.DonnerTableauHide(j, i);
                       
                        if (nombre == 10)
                        {
                            nombre = 2;
                        }
                        else nombre += 3;
                        Rectangle sourceRecHide = new Rectangle(128 * nombre, 0, 128, 128);
                        Rectangle destRecHide = new Rectangle(positionTuile.X + (128 / scale) * i,
                                                              positionTuile.Y + (128 / scale) * j,
                                                              128 / scale, 128 / scale);
                        spriteTuile.Draw(tileTexture, destRecHide, sourceRecHide, Color.White);

                        if (demineur.DonnerTableauShow(j, i) == 'X')
                        {
                            int ValueTile;
                            if ((tuileMaintenu[0] == i) && (tuileMaintenu[1] == j) && !Perdu)
                            {
                                ValueTile = 3;
                            }
                            else
                            {
                                ValueTile = 0;
                            }
                            /////////////////////////// Desinner Tableau Show ///////////////////////////
                            Rectangle sourceRectangle = new Rectangle(128 * ValueTile, 0, 128, 128);
                            Rectangle destinationRectangle = new Rectangle(positionTuile.X + (128 / scale) * i,
                                                                           positionTuile.Y + (128 / scale) * j,
                                                                           128 / scale, 128 / scale);
                            spriteTuile.Draw(tileTexture, destinationRectangle, sourceRectangle, Color.White);
                            tileRectangles.Add(destinationRectangle);
                        }else if(demineur.DonnerTableauShow(j, i) == 'F')
                        {
                            Rectangle sourceRectangle = new Rectangle(128 * 1, 0, 128, 128);
                            Rectangle destinationRectangle = new Rectangle(positionTuile.X + (128 / scale) * i,
                                                                           positionTuile.Y + (128 / scale) * j,
                                                                           128 / scale, 128 / scale);
                            spriteTuile.Draw(tileTexture, destinationRectangle, sourceRectangle, Color.White);
                            tileRectangles.Add(destinationRectangle);
                        }
                    }
                    else
                    {
                        int ValueTile;
                        if ((tuileMaintenu[0] == i) && (tuileMaintenu[1] == j))
                        {
                            ValueTile = 3;
                        }
                        else
                        {
                            ValueTile = 0;
                        }
                        /////////////////////////// Desinner Tableau Show ///////////////////////////
                        Rectangle sourceRectangle = new Rectangle(128 * ValueTile, 0, 128, 128);
                        Rectangle destinationRectangle = new Rectangle(positionTuile.X + (128 / scale) * i,
                                                                        positionTuile.Y + (128 / scale) * j,
                                                                        128 / scale, 128 / scale);
                        spriteTuile.Draw(tileTexture, destinationRectangle, sourceRectangle, Color.White);
                        tileRectangles.Add(destinationRectangle);
                        
                    }
                }
            }

            int NbBombe = demineur.NbDeBombe();
            char[] BombeRestante = (NbBombe.ToString("D3")).ToCharArray();
            scale = 1;
            if (NbBombe >= 0)
            {
                for (int i = 0; i < BombeRestante.Length; i++)
                {
                    int valeur = int.Parse(BombeRestante[i].ToString());
                    Rectangle sourceRectangle = new Rectangle(13 * valeur, 0, 13, 23);
                    Rectangle destinationRectangle = new Rectangle(positionBombes.X + 13 * scale * i, positionBombes.Y, 13 * scale, 23 * scale);
                    spriteTuile.Draw(chiffresTexture, destinationRectangle, sourceRectangle, Color.White);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Rectangle sourceRectangle = new Rectangle(13 * 0, 0, 13, 23);
                    Rectangle destinationRectangle = new Rectangle(positionBombes.X + 13 * scale * i, positionBombes.Y, 13 * scale, 23 * scale);
                    spriteTuile.Draw(chiffresTexture, destinationRectangle, sourceRectangle, Color.White);
                }
            }

            char[] TempPassee = (compteur.ToString("D3")).ToCharArray();
            for (int i = 0; i < TempPassee.Length; i++)
            {
                int valeur = int.Parse(TempPassee[i].ToString());
                Rectangle sourceRectangle = new Rectangle(13 * valeur, 0, 13, 23);
                Rectangle destinationRectangle = new Rectangle(positionTemps.X + 13 * scale * i, positionTemps.Y , 13 * scale , 23 * scale);
                spriteTuile.Draw(chiffresTexture, destinationRectangle, sourceRectangle, Color.White);
            }

            scale = 1;         
            
            if (Perdu)
            {
                Rectangle sourceRectangle = new Rectangle(26 * 2, 0, 26, 26);
                destinationRectangleVisage = new Rectangle(positionVisage.X, positionVisage.Y, 26 * scale, 26 * scale);
                spriteTuile.Draw(visageTexture, destinationRectangleVisage, sourceRectangle, Color.White);
            }
            else if (ClicDroit)
            {
                Rectangle sourceRectangle = new Rectangle(26 * 1, 0, 26, 26);
                destinationRectangleVisage = new Rectangle(positionVisage.X, positionVisage.Y, 26 * scale, 26 * scale);
                spriteTuile.Draw(visageTexture, destinationRectangleVisage, sourceRectangle, Color.White);
            }
            else if (VisageClique)
            {
                Rectangle sourceRectangle = new Rectangle(26 * 3, 0, 26, 26);
                destinationRectangleVisage = new Rectangle(positionVisage.X, positionVisage.Y, 26 * scale, 26 * scale);
                spriteTuile.Draw(visageTexture, destinationRectangleVisage, sourceRectangle, Color.White);
            }
            else if (JeuTermine)
            {
                Rectangle sourceRectangle = new Rectangle(26 * 4, 0, 26, 26);
                destinationRectangleVisage = new Rectangle(positionVisage.X, positionVisage.Y, 26 * scale, 26 * scale);
                spriteTuile.Draw(visageTexture, destinationRectangleVisage, sourceRectangle, Color.White);
            }
            else
            {
                Rectangle sourceRectangle = new Rectangle(0, 0, 26, 26);
                destinationRectangleVisage = new Rectangle(positionVisage.X, positionVisage.Y, 26 * scale, 26 * scale);
                spriteTuile.Draw(visageTexture, destinationRectangleVisage, sourceRectangle, Color.White);
            }

            if (DifficulteClique)
            {
                int indice = Difficulte % 3 * 2 +1;

                Rectangle sourceRectangleDifficulte = new Rectangle(26 * indice, 0, 26, 26);
                destinationRectangleDifficulte = new Rectangle(positionDifficulte.X, positionDifficulte.Y, 26 * scale, 26 * scale);
                spriteTuile.Draw(niveauTexture, destinationRectangleDifficulte, sourceRectangleDifficulte, Color.White);
            }
            else
            {

                int indice = Difficulte % 3 * 2;
                Rectangle sourceRectangleDifficulte = new Rectangle(26 * indice, 0, 26, 26);
                destinationRectangleDifficulte = new Rectangle(positionDifficulte.X, positionDifficulte.Y, 26 * scale, 26 * scale);
                spriteTuile.Draw(niveauTexture, destinationRectangleDifficulte, sourceRectangleDifficulte, Color.White);
            }


            spriteTuile.End();
            base.Draw(gameTime);
        }

        protected void Compteur(CancellationToken token)
        {
            compteur = 0;
            while (!token.IsCancellationRequested)
            {
                compteur++;
                Thread.Sleep(1000); 
            }
            Console.WriteLine("Compteur terminé !");
        }

        private Texture2D CreateGradientTexture(GraphicsDevice device, int width, int height, Color startColor, Color endColor)
        {
            Texture2D texture = new Texture2D(device, width, height);
            Color[] data = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float t = (x + y) / (float)(width + height);

                    byte r = (byte)(startColor.R + t * (endColor.R - startColor.R));
                    byte g = (byte)(startColor.G + t * (endColor.G - startColor.G));
                    byte b = (byte)(startColor.B + t * (endColor.B - startColor.B));

                    data[y * width + x] = new Color(r, g, b);
                }
            }

            texture.SetData(data);
            return texture;
        }
    }
}
