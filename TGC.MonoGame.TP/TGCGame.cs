using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Constraints.Contact;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Content.Geometries;
using TGC.MonoGame.TP.Physics;
using NumericVector3 = System.Numerics.Vector3;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Vector3 = Microsoft.Xna.Framework.Vector3;

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
        //private Matrix View { get; set; }
        //private Matrix Projection { get; set; }
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

        //BORRAR DESDE ACA
        private List<Matrix> ActiveBoxesWorld;

        private List<BodyHandle> BoxHandles;

        private bool CanShoot = true;

        private CubePrimitive cubePrimitive;

        private List<Matrix> InactiveBoxesWorld;

        private List<float> Radii;

        private Random Random;

        private List<BodyHandle> SphereHandles;

        private SpherePrimitive spherePrimitive;

        private List<Matrix> SpheresWorld;
        //HASTA ACA

        private BodyHandle CarHandle;

        private Matrix CarWorldPhysics;
        private Matrix CarWorldPhysics2;

        /// <summary>
        ///     Gets the simulation created by the demo's Initialize call.
        /// </summary>
        public Simulation Simulation { get; protected set; }

        //Note that the buffer pool used by the simulation is not considered to be *owned* by the simulation. The simulation merely uses the pool.
        //Disposing the simulation will not dispose or clear the buffer pool.
        /// <summary>
        ///     Gets the buffer pool used by the demo's simulation.
        /// </summary>
        public BufferPool BufferPool { get; private set; }

        /// <summary>
        ///     Gets the thread dispatcher available for use by the simulation.
        /// </summary>
        public SimpleThreadDispatcher ThreadDispatcher { get; private set; }

        Vector3 Posicion = Vector3.Zero;
        Vector3 CarPosicion = Vector3.Zero;
        Quaternion CarRotation = Quaternion.Identity;
        Matrix rotationMatrix = Matrix.Identity;
        double Rotation = 0;
        float CameraSpeed = 500;
        float CarSpeed = 40;
        Vector3 velocidad = Vector3.Zero;
        Vector3 velocidadV = Vector3.Zero;


        CollidableProperty<OurCarBodyProperties> properties;
        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            properties = new CollidableProperty<OurCarBodyProperties>();
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
            //View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up) * Matrix.CreateTranslation(Vector3.Down * 100);
            //Projection =
            //    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);
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

            // Physics

            BufferPool = new BufferPool();

            //It may be worth using something like hwloc to extract extra information to reason about.
            var targetThreadCount = Math.Max(1,
                Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);


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

            /*
            var effect = CarEffect as BasicEffect;

            effect.Texture = CarTexture;

            effect = Effect as BasicEffect;

            effect.Texture = FloorTexture;
            */
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

            //Physics

            Simulation = Simulation.Create(BufferPool, new NarrowPhaseCallbacks() { bodyProperties = properties},
                new PoseIntegratorCallbacks(new NumericVector3(0, -10, 0)), new PositionFirstTimestepper());


            var carShape = new Box(5, 5, 10);
            carShape.ComputeInertia(100, out var carInertia);
            var carIndex = Simulation.Shapes.Add(carShape);

            CarHandle = Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                new NumericVector3(0f, 0f, 0f),
                carInertia,
                new CollidableDescription(carIndex, 0.1f),
                new BodyActivityDescription(0.01f)
                
                ));
            ref var bodyProperties = ref properties.Allocate(CarHandle);
            bodyProperties = new OurCarBodyProperties();
            bodyProperties.Friction = 10;
            var carReference = Simulation.Bodies.GetBodyReference(CarHandle);
            var carPosition = carReference.Pose.Position;
            var carQuaternion = carReference.Pose.Orientation;
            CarWorldPhysics = Matrix.CreateFromQuaternion(new Quaternion(carQuaternion.X, carQuaternion.Y, carQuaternion.Z,
                       carQuaternion.W)) * Matrix.CreateTranslation(new Vector3(carPosition.X, carPosition.Y, carPosition.Z));

            // BORRAR DESDE ACA

            SphereHandles = new List<BodyHandle>();
            ActiveBoxesWorld = new List<Matrix>();
            InactiveBoxesWorld = new List<Matrix>();
            SpheresWorld = new List<Matrix>();
            Random = new Random();
            BoxHandles = new List<BodyHandle>(800);
            Radii = new List<float>();

            var boxShape = new Box(1, 1, 1);
            boxShape.ComputeInertia(1, out var boxInertia);
            var boxIndex = Simulation.Shapes.Add(boxShape);
            const int pyramidCount = 40;
            for (var pyramidIndex = 0; pyramidIndex < pyramidCount; ++pyramidIndex)
            {
                const int rowCount = 20;
                for (var rowIndex = 0; rowIndex < rowCount; ++rowIndex)
                {
                    var columnCount = rowCount - rowIndex;
                    for (var columnIndex = 0; columnIndex < columnCount; ++columnIndex)
                    {
                        var bh = Simulation.Bodies.Add(BodyDescription.CreateDynamic(
                            new NumericVector3((-columnCount * 0.5f + columnIndex) * boxShape.Width,
                                (rowIndex + 0.5f) * boxShape.Height,
                                (pyramidIndex - pyramidCount * 0.5f) * (boxShape.Length + 4)),
                            boxInertia,
                            new CollidableDescription(boxIndex, 0.1f),
                            new BodyActivityDescription(0.01f)));
                        BoxHandles.Add(bh);
                    }
                }
            }

            //Prevent the boxes from falling into the void.

            var BaseHandle = Simulation.Statics.Add(new StaticDescription(new NumericVector3(0, -0.5f, 0),
                new CollidableDescription(Simulation.Shapes.Add(new Box(25000, 1, 25000)), 0.1f)));
            ref var BaseBodyProperties = ref properties.Allocate(BaseHandle);
            BaseBodyProperties = new OurCarBodyProperties();
            BaseBodyProperties.Friction = 0.1f;


            cubePrimitive = new CubePrimitive(GraphicsDevice, 1f, Color.White);

            spherePrimitive = new SpherePrimitive(GraphicsDevice);

            var count = BoxHandles.Count;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            for (var index = 0; index < count; index++)
            {
                var bodyHandle = BoxHandles[index];
                var bodyReference = Simulation.Bodies.GetBodyReference(bodyHandle);
                var position = bodyReference.Pose.Position;
                var quaternion = bodyReference.Pose.Orientation;
                var world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

                cubePrimitive.Draw(world, FollowCamera.View, FollowCamera.Projection);
            }

            cubePrimitive.Draw(CarWorldPhysics, FollowCamera.View, FollowCamera.Projection);

            //HASTA ACA 

            base.LoadContent();
        }

        private NumericVector3 vectorization (Vector3 vec)
        {
            return new NumericVector3(vec.X, vec.Y, vec.Z);
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        
        protected override void Update(GameTime gameTime)
        {
            Simulation.Timestep(1 / 60f, ThreadDispatcher);
            var carReference = Simulation.Bodies.GetBodyReference(CarHandle);

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

            var carPosition = carReference.Pose.Position;
            var carQuaternion = carReference.Pose.Orientation;
            CarWorldPhysics = Matrix.CreateScale(0.2f) * Matrix.CreateFromQuaternion(new Quaternion(carQuaternion.X, carQuaternion.Y, carQuaternion.Z,
                       carQuaternion.W)) * Matrix.CreateTranslation(new Vector3(carPosition.X, carPosition.Y, carPosition.Z));
            var speed = 300f;
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && CarPosicion.Y <= 0)
            {
                carReference.ApplyLinearImpulse(new NumericVector3(0f, speed, 0f) * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                //carReference.ApplyLinearImpulse(new NumericVector3(0f, 0f, speed) * (float)gameTime.ElapsedGameTime.TotalSeconds);
                carReference.ApplyAngularImpulse(new NumericVector3 (0,36,0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                //carReference.ApplyLinearImpulse(new NumericVector3(0f, 0f, -speed) * (float)gameTime.ElapsedGameTime.TotalSeconds);
                carReference.ApplyAngularImpulse(new NumericVector3( 0, -36, 0));

            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                carReference.ApplyLinearImpulse(vectorization(-CarWorldPhysics.Forward) * speed);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                carReference.ApplyLinearImpulse(vectorization(CarWorldPhysics.Forward) * speed );

                //carReference.ApplyLinearImpulse(new NumericVector3(speed, 0f, 0f) * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            velocidad -= velocidad * new Vector3(1, 0, 1) * 3 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            CarPosicion += velocidad + velocidadV;

            CarMatrix = Matrix.CreateScale(0.05f)
                        * Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitY, getAngle(velocidad)))
                        * (Matrix.CreateTranslation(CarPosicion));
            
            // Capturar Input teclado
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
                Exit();

            // Actualizo la camara, enviandole la matriz de mundo del auto
            FollowCamera.Update(gameTime, CarWorldPhysics);

            //CarWorldPhysics = Matrix.CreateFromQuaternion(new Quaternion(carQuaternion.X, carQuaternion.Y, carQuaternion.Z,
            //  carQuaternion.W)) * Matrix.CreateTranslation(new Vector3(carPosition.X, carPosition.Y, carPosition.Z));

            
            //BORRRAR DE ACA

            if (Keyboard.GetState().IsKeyDown(Keys.Z) && CanShoot)
            {
                CanShoot = false;
                //Create the shape that we'll launch at the pyramids when the user presses a button.
                var radius = 0.5f + 5 * (float)Random.NextDouble();
                var bulletShape = new Sphere(radius);

                //Note that the use of radius^3 for mass can produce some pretty serious mass ratios. 
                //Observe what happens when a large ball sits on top of a few boxes with a fraction of the mass-
                //the collision appears much squishier and less stable. For most games, if you want to maintain rigidity, you'll want to use some combination of:
                //1) Limit the ratio of heavy object masses to light object masses when those heavy objects depend on the light objects.
                //2) Use a shorter timestep duration and update more frequently.
                //3) Use a greater number of solver iterations.
                //#2 and #3 can become very expensive. In pathological cases, it can end up slower than using a quality-focused solver for the same simulation.
                //Unfortunately, at the moment, bepuphysics v2 does not contain any alternative solvers, so if you can't afford to brute force the the problem away,
                //the best solution is to cheat as much as possible to avoid the corner cases.
                var velocity = Vector3.Forward * 200;
                var position = new NumericVector3(CarPosicion.X, CarPosicion.Y, CarPosicion.Z);
                var bodyDescription = BodyDescription.CreateConvexDynamic(position,
                    new BodyVelocity(new NumericVector3(velocity.X, velocity.Y, velocity.Z)),
                    bulletShape.Radius * bulletShape.Radius * bulletShape.Radius, Simulation.Shapes, bulletShape);

                var bodyHandle = Simulation.Bodies.Add(bodyDescription);

                Radii.Add(radius);
                SphereHandles.Add(bodyHandle);
            }

            if (Keyboard.GetState().IsKeyUp(Keys.Z)) CanShoot = true;

            ActiveBoxesWorld.Clear();
            InactiveBoxesWorld.Clear();
            var count = BoxHandles.Count;
            for (var index = 0; index < count; index++)
            {
                var bodyHandle = BoxHandles[index];
                var bodyReference = Simulation.Bodies.GetBodyReference(bodyHandle);
                var position = bodyReference.Pose.Position;
                var quaternion = bodyReference.Pose.Orientation;
                var world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));

                if (bodyReference.Awake)
                    ActiveBoxesWorld.Add(world);
                else
                    InactiveBoxesWorld.Add(world);
            }

            SpheresWorld.Clear();
            var sphereCount = SphereHandles.Count;
            for (var index = 0; index < sphereCount; index++)
            {
                var bodyHandle = SphereHandles[index];
                var bodyReference = Simulation.Bodies.GetBodyReference(bodyHandle);
                var position = bodyReference.Pose.Position;
                var quaternion = bodyReference.Pose.Orientation;
                var world =
                    Matrix.CreateFromQuaternion(new Quaternion(quaternion.X, quaternion.Y, quaternion.Z,
                        quaternion.W)) * Matrix.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
                SpheresWorld.Add(world);
            }
            //HASTA ACA

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
            Quad.Draw(FloorWorld, FollowCamera.View, FollowCamera.Projection);

            // Aca deberiamos poner toda la logia de renderizado del juego.

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(FollowCamera.View);
            Effect.Parameters["Projection"].SetValue(FollowCamera.Projection);

            CarEffect.Parameters["View"].SetValue(FollowCamera.View);
            CarEffect.Parameters["Projection"].SetValue(FollowCamera.Projection);

            foreach (var mesh in CarModel.Meshes)
            {
                var Mesh = mesh.ParentBone.Transform;
                CarEffect.Parameters["World"].SetValue(Mesh * Matrix.CreateScale(0.1f) * CarWorldPhysics);
                mesh.Draw();
            }


            //foreach (var mesh in CombatVehicleModel.Meshes)
            //{
            //    var Mesh = mesh.ParentBone.Transform;
            //    Effect.Parameters["World"].SetValue(Mesh * CombatVehicleWorld);
            //    mesh.Draw();
            //}

            //foreach (var mesh in TankModel.Meshes)
            //{
            //    var Mesh = mesh.ParentBone.Transform;
            //    Effect.Parameters["World"].SetValue(Mesh * TankWorld);
            //    mesh.Draw();
            //}

            //foreach (var mesh in BarrelModel.Meshes)
            //{
            //    var Mesh = mesh.ParentBone.Transform;
            //    Effect.Parameters["World"].SetValue(Mesh * BarrelWorld);
            //    mesh.Draw();
            //}

            //foreach (var mesh in BoxesModel.Meshes)
            //{
            //    var Mesh = mesh.ParentBone.Transform;
            //    Effect.Parameters["World"].SetValue(Mesh * BoxesWorld);
            //    mesh.Draw();
            //}

            //foreach (var mesh in CarrotModel.Meshes)
            //{
            //    var Mesh = mesh.ParentBone.Transform;
            //    Effect.Parameters["World"].SetValue(Mesh * CarrotWorld);
            //    mesh.Draw();
            //}

            //borrar
            var count = BoxHandles.Count;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            cubePrimitive.Effect.DiffuseColor = new Vector3(1f, 0f, 0f);
            ActiveBoxesWorld.ForEach(boxWorld => cubePrimitive.Draw(boxWorld, FollowCamera.View, FollowCamera.Projection));
            cubePrimitive.Effect.DiffuseColor = new Vector3(0.1f, 0.1f, 0.3f);
            InactiveBoxesWorld.ForEach(boxWorld => cubePrimitive.Draw(boxWorld, FollowCamera.View, FollowCamera.Projection));

            SpheresWorld.ForEach(sphereWorld => spherePrimitive.Draw(sphereWorld, FollowCamera.View, FollowCamera.Projection));
            //HASTA ACA

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