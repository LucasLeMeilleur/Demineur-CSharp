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

        Texture2D tileTexture;
        Texture2D chiffresTexture;
        Texture2D visageTexture;
        Texture2D niveauTexture;
        private GraphicsDeviceManager graphics;
        Vector2 tilePosition;
        Vector2 chiffresPosition;
        Vector2 visagePosition;
        Vector2 niveauPosition;
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
        private CancellationTokenSource cts = new CancellationTokenSource();
        private bool PartieDemarre = false;
        private bool DifficulteClique=false;

        public Game1()
        {
            demineur = new Bombe(Difficulte);
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 900; // Largeur
            graphics.PreferredBackBufferHeight = 600; // Hauteur
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
            int offsetX = 50, offsetY = 50; 
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
                    tileRectangles.Clear();
                    JeuInitialise = false;
                    PremierClick = false;
                    demineur = null;
                    Debug.WriteLine("Recommencé");
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
                    JeuInitialise = false;
                    PremierClick = false;
                    demineur = null;
                    Debug.WriteLine("Recommencé");
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
                            demineur.InitialiserJeu(tuileY, tuileX);
                            demineur.DevoilerLesCases(tuileY, tuileX);
                            PremierClick = true;
                            JeuInitialise = true;

                            threadCompteur = new Thread(() => Compteur(cts.Token));
                            threadCompteur.Start();
                            Debug.WriteLine("Intialisé");
                            PartieDemarre = true;
                            Debug.WriteLine(tuileY + " " + tuileX);
                        }
                        else
                        {
                            demineur.DecouvrirUneCase(tuileY, tuileX);

                            if (demineur.DonnerTableauHide(tuileY, tuileX) == 10)
                            {
                                demineur.RevelerLesBombes();
                                Perdu = true;
                            }
                            Debug.WriteLine("Valeur pour " + tuileY + ", " + tuileX + " : " + demineur.DonnerTableauHide(tuileY, tuileX));

                            demineur.TableauShow();
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

            if (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released && PremierClick)
            {
                tuileX = (mouseState.X - offsetX) / tileSize;
                tuileY = (mouseState.Y - offsetY) / tileSize;
                if (tuileX >= 0 && tuileX < demineur.LargeurTableau() &&
                    tuileY >= 0 && tuileY < demineur.HauteurTableau())
                {

                    if (demineur.DonnerTableauShow(tuileX, tuileY) == 'X')
                    {
                        demineur.Flag(tuileX, tuileY);
                    }
                    else if (demineur.DonnerTableauShow(tuileX, tuileY) == 'F')
                    {
                        demineur.RetirerSupposition(tuileX, tuileY);
                    }
                    else
                    {
                        Debug.WriteLine(demineur.DonnerTableauShow(tuileX, tuileY));
                    }
                }
            }
            previousMouseState = mouseState;

            if (PartieDemarre)
            {
                if (demineur.Fini())
                {
                    cts.Cancel();
                    threadCompteur.Join();
                }
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
            // TODO: use this.Content to load your game content here
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(136, 136, 136));

            int scale = 5;
            int NumTuile = 0;


            Point positionTuile = new Point(50, 50);
            Point positionBombes = new Point(0, 0);
            Point positionTemps = new Point(200, 0);
            Point positionVisage = new Point(80, 0);
            Point positionDifficulte = new Point(250, 0);

            spriteTuile.Begin();

            tileRectangles.Clear(); // On nettoie la liste avant de la remplir

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
                            Rectangle sourceRectangle = new Rectangle(128 * NumTuile, 0, 128, 128);
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
            for (int i = 0; i < BombeRestante.Length; i++)
            {
                int valeur = int.Parse(BombeRestante[i].ToString());

                Rectangle sourceRectangle = new Rectangle(13 * valeur, 0, 13, 23);
                Rectangle destinationRectangle = new Rectangle(positionBombes.X + 13*scale*i, positionBombes.Y , 13 * scale , 23 * scale);
                spriteTuile.Draw(chiffresTexture, destinationRectangle, sourceRectangle, Color.White);
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
                Thread.Sleep(1000);  // Attend 1 seconde (1000 ms)
            }
            Console.WriteLine("Compteur terminé !");
        }
    }
}
