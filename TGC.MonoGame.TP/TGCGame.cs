using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Geometries.Textures;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        public const float Speed = 40f;

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            // Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model Model { get; set; }
        private Effect Effect { get; set; }
        private Effect CarEffect { get; set; }
        private Effect TilingEffect { get; set; }
        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }
        private FollowCamera FollowCamera { get; set; }
        private Model CarModel { get; set; }
        private Matrix CarMatrix { get; set; }
        private Texture2D CarTexture { get; set; }
        private QuadPrimitive Quad { get; set; }
        private Texture2D FloorTexture { get; set; }
        private Matrix FloorWorld { get; set; }
        private Matrix MovimientoCamara { get; set; }
        private Matrix CombatVehicleWorld { get; set; }
        private Model CombatVehicleModel { get; set; }
        private Matrix TankWorld { get; set; }
        private Model TankModel { get; set; }
        private Model BarrelModel { get; set; }
        private Matrix BarrelWorld { get; set; }
        private Model BoxesModel { get; set; }
        private Matrix BoxesWorld { get; set; }
        private Model CarrotModel { get; set; }
        private Matrix CarrotWorld { get; set; }
        private List<Model> Models { get; set; }

        Vector3 Posicion = Vector3.Zero;
        Vector3 CarPosicion = Vector3.Zero;
        Quaternion CarRotation = Quaternion.Identity;
        Matrix rotationMatrix = Matrix.Identity;
        double Rotation = 0;
        float CameraSpeed = 500;
        float CarSpeed = 40;
        Vector3 velocidad = Vector3.Zero;
        Vector3 velocidadV = Vector3.Zero;

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
            // Configuro el tamaño de la pantalla
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            Graphics.ApplyChanges();

            // Creo una camaar para seguir a nuestro auto
            FollowCamera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio);


            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;
            // Seria hasta aca.

            // Configuramos nuestras matrices de la escena.
            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up) * Matrix.CreateTranslation(Vector3.Down * 100);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            CarMatrix = Matrix.Identity;
            // Create World matrices for the Floor and Box
            FloorWorld = Matrix.CreateScale(2000f, 0.001f, 2000f);
            MovimientoCamara = Matrix.Identity;

            // Modelos decorativos

            CombatVehicleWorld = Matrix.CreateScale(0.5f) * Matrix.Identity * Matrix.CreateTranslation(Vector3.Left * 10);

            TankWorld = Matrix.CreateScale(.1f) * Matrix.Identity * Matrix.CreateTranslation(Vector3.Right * 100 + Vector3.Up * 40 + Vector3.Backward * 100);

            BarrelWorld = Matrix.Identity * Matrix.CreateScale(15f) * Matrix.CreateTranslation(Vector3.Forward * 100 + Vector3.Up * 15);
            BoxesWorld = Matrix.Identity * Matrix.CreateScale(0.3f) * Matrix.CreateTranslation(Vector3.Left * 100 + Vector3.Backward * 50 + Vector3.Up * 15);
            CarrotWorld = Matrix.Identity * Matrix.CreateScale(0.3f) * Matrix.CreateTranslation(Vector3.Right * 100 + Vector3.Down * 10);


            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargo el modelo del logo.
            Model = Content.Load<Model>(ContentFolder3D + "tgc-logo/tgc-logo");
            CarModel = Content.Load<Model>(ContentFolder3D + "racingcara/RacingCar");
            CombatVehicleModel = Content.Load<Model>(ContentFolder3D + "CombatVehicle/Vehicle");
            TankModel = Content.Load<Model>(ContentFolder3D + "tank/tank");
            BarrelModel = Content.Load<Model>(ContentFolder3D + "modelosInternet/barrel");
            BoxesModel = Content.Load<Model>(ContentFolder3D + "modelosInternet/boxes");
            CarrotModel = Content.Load<Model>(ContentFolder3D + "modelosInternet/Carrot");

            // Create the Quad
            //TilingEffect = Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            //TilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));
            FloorTexture = Content.Load<Texture2D>(ContentFolderTextures + "tierra");
            Quad = new QuadPrimitive(GraphicsDevice);

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            CarEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");


            CarTexture = Content.Load<Texture2D>(ContentFolder3D + "racingcara/Vehicle_metallic");

            //var effect = Effect as BasicEffect;

            //effect.Texture = CarTexture;

            foreach (var mesh in CarModel.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = CarEffect;
            }

            foreach (var mesh in CombatVehicleModel.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = CarEffect;
            }

            foreach (var mesh in TankModel.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = CarEffect;
            }

            foreach (var mesh in BarrelModel.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = CarEffect;
            }

            foreach (var mesh in BoxesModel.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = CarEffect;
            }

            foreach (var mesh in CarrotModel.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = CarEffect;
            }

            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            MovimientoCamara = Matrix.Identity;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) {
                MovimientoCamara = Matrix.CreateRotationY((float)(Math.PI * gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                MovimientoCamara = Matrix.CreateRotationY((float)(-Math.PI * gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                MovimientoCamara = Matrix.CreateRotationX((float)(-Math.PI * gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                MovimientoCamara = Matrix.CreateRotationX((float)(Math.PI * gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up) * Matrix.CreateTranslation(Vector3.Down * 100);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                velocidad += AddSpeed(gameTime, Vector3.Left);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                velocidad += AddSpeed(gameTime, Vector3.Right);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                velocidad += AddSpeed(gameTime, Vector3.Forward);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                velocidad += AddSpeed(gameTime, Vector3.Backward);
            }
            if (CarPosicion.Y > 0)
            {
                velocidadV.Y -= 20 * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                velocidadV.Y = 0;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && CarPosicion.Y <= 0)
            {
                velocidadV.Y += 10;
            }

            velocidad -= velocidad * new Vector3(1, 0, 1) * 3 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            CarPosicion += velocidad + velocidadV;

            CarMatrix = Matrix.CreateScale(0.3f)
                        * Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitY, getAngle(velocidad)))
                        * (Matrix.CreateTranslation(CarPosicion) /** Matrix.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 2)*/);

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            // Basado en el tiempo que paso se va generando una rotacion.

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Violet);
            // Floor drawing

            //Set the Technique inside the TilingEffect to "BaseTiling", we want to control the tiling on the floor
            //Using its original Texture Coordinates
            //TilingEffect.CurrentTechnique = TilingEffect.Techniques["BaseTiling"];
            //// Set the Tiling value
            //TilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));
            //// Set the WorldViewProjection matrix
            //TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * Projection);
            //// Set the Texture that the Floor will use
            //TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
            //Quad.Draw(TilingEffect);
            //QuadPrimitive quad = new QuadPrimitive(GraphicsDevice);
            Quad.Draw(FloorWorld, View, Projection);

            // Aca deberiamos poner toda la logia de renderizado del juego.

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            //Effect.Parameters["DiffuseColor"].SetValue(Color.DarkBlue.ToVector3());
            //var rotationMatrix = Matrix.CreateRotationY(Rotation);

            CarEffect.Parameters["View"].SetValue(View);
            CarEffect.Parameters["Projection"].SetValue(Projection);

            foreach (var mesh in CarModel.Meshes)
            {
                //CarMatrix = mesh.ParentBone.Transform /** rotationMatrix */* Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(Vector3.Backward * 100);
                var Mesh = mesh.ParentBone.Transform;
                Effect.Parameters["World"].SetValue(Mesh * CarMatrix);
                mesh.Draw();
            }

            foreach (var mesh in CombatVehicleModel.Meshes)
            {
                var Mesh = mesh.ParentBone.Transform;
                Effect.Parameters["World"].SetValue(Mesh * CombatVehicleWorld);
                mesh.Draw();
            }

            foreach (var mesh in TankModel.Meshes)
            {
                var Mesh = mesh.ParentBone.Transform;
                Effect.Parameters["World"].SetValue(Mesh * TankWorld);
                mesh.Draw();
            }
            
            foreach (var mesh in BarrelModel.Meshes)
            {
                var Mesh = mesh.ParentBone.Transform;
                Effect.Parameters["World"].SetValue(Mesh * BarrelWorld);
                mesh.Draw();
            }

            foreach (var mesh in BoxesModel.Meshes)
            {
                var Mesh = mesh.ParentBone.Transform;
                Effect.Parameters["World"].SetValue(Mesh * BoxesWorld);
                mesh.Draw();
            }

            foreach (var mesh in CarrotModel.Meshes)
            {
                var Mesh = mesh.ParentBone.Transform;
                Effect.Parameters["World"].SetValue(Mesh * CarrotWorld);
                mesh.Draw();
            }

            View *= MovimientoCamara;

        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }

        private Vector3 AddSpeed(GameTime gameTime, Vector3 dir)
        {
            return dir * (float)(CarSpeed * gameTime.ElapsedGameTime.TotalSeconds);
        }

        float getAngle(Vector3 dir)
        {
            return (float)Math.Atan2(dir.X, dir.Z);
        }
    }
}