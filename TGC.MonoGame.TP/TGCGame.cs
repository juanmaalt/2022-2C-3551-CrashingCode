using System;
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
        public const string ContentRacingCar = ContentFolder3D + "RacingCarA/";

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
        private float Rotation { get; set; }
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
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
            CarMatrix = Matrix.Identity;
            // Create World matrices for the Floor and Box
            FloorWorld = Matrix.CreateScale(200f, 0.001f, 200f);
            MovimientoCamara = Matrix.Identity;

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

            // Create the Quad
            Quad = new QuadPrimitive(GraphicsDevice);
            TilingEffect = Content.Load<Effect>(ContentFolderEffects + "TextureTiling");
            TilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));
            FloorTexture = Content.Load<Texture2D>(ContentFolderTextures + "tierra");

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            CarEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
           

            CarTexture = Content.Load<Texture2D>(ContentFolder3D + "racingcara/Vehicle_normal");

            //var effect = Effect as BasicEffect;

            //effect.Texture = CarTexture;

            foreach (var mesh in CarModel.Meshes)
            {
                // A mesh contains a collection of parts
                foreach (var meshPart in mesh.MeshParts)
                    // Assign the loaded effect to each part
                    meshPart.Effect = CarEffect;
            }
            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            foreach (var mesh in Model.Meshes)
                // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
            foreach (var meshPart in mesh.MeshParts)
                meshPart.Effect = Effect;

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
            if (Keyboard.GetState().IsKeyDown(Keys.A)) {
                MovimientoCamara = Matrix.CreateRotationY((float)(Math.PI * gameTime.ElapsedGameTime.TotalSeconds));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                MovimientoCamara = Matrix.CreateRotationX((float)(Math.PI * gameTime.ElapsedGameTime.TotalSeconds));
            }

            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            // Basado en el tiempo que paso se va generando una rotacion.
            Rotation += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Floor drawing

            // Set the Technique inside the TilingEffect to "BaseTiling", we want to control the tiling on the floor
            // Using its original Texture Coordinates
            TilingEffect.CurrentTechnique = TilingEffect.Techniques["BaseTiling"];
            // Set the Tiling value
            TilingEffect.Parameters["Tiling"].SetValue(new Vector2(10f, 10f));
            // Set the WorldViewProjection matrix
            TilingEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * Projection);
            // Set the Texture that the Floor will use
            TilingEffect.Parameters["Texture"].SetValue(FloorTexture);
            Quad.Draw(TilingEffect);

            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Cyan);

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(View);
            Effect.Parameters["Projection"].SetValue(Projection);
            //Effect.Parameters["DiffuseColor"].SetValue(Color.DarkBlue.ToVector3());
            var rotationMatrix = Matrix.CreateRotationY(Rotation);

            CarEffect.Parameters["View"].SetValue(View);
            CarEffect.Parameters["Projection"].SetValue(Projection);

            foreach (var mesh in CarModel.Meshes)
            {
                CarMatrix = mesh.ParentBone.Transform * rotationMatrix *Matrix.CreateTranslation(Vector3.Up * -50) * Matrix.CreateScale(0.2f);
                Effect.Parameters["World"].SetValue(CarMatrix);
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
    }
}