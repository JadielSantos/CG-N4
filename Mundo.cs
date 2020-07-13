/**
  Autor: Jadiel dos Santos e Matheus Soares
**/

#define CG_Gizmo
#define CG_Privado

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK.Input;
using CG_Biblioteca;
using Timer = System.Timers.Timer;

//TODO: arrumar o id dos objetos usando char letra = 'A'; letra++;
//TODO: ter mais objetos geométricos: esfera
//TODO: arrumar objeto cone
//TODO: ter iluminação
//TODO: ter textura
//TODO: ter texto 2D
//TODO: ter um mapa em 2D
//TODO: ler arquivo OBJ/MTL
//TODO: ter audio
//TODO: usar DisplayList
//TODO: Seleciona Alpha
//TODO: Unproject
namespace gcgcg
{
  class Mundo : GameWindow
  {
    private static Mundo instanciaMundo = null;

    private Mundo(int width, int height) : base(width, height) { }

    public static Mundo GetInstance(int width, int height)
    {
      if (instanciaMundo == null)
        instanciaMundo = new Mundo(width, height);
      return instanciaMundo;
    }

    private CameraPerspective camera = new CameraPerspective();
    protected List<Objeto> objetosLista = new List<Objeto>();
    private ObjetoGeometria objetoSelecionado = null;
    private bool bBoxDesenhar = false;
    int mouseX, mouseY;   //TODO: achar método MouseDown para não ter variável Global
    private Poligono objetoNovo = null;
    private String objetoId = "A";
    private Retangulo obj_Retangulo;
    private float PaddleFrenteXLeft = -45;
    private float PaddleFrenteXRight = 45;
    private float PaddleFundoXLeft = -45;
    private float PaddleFundoXRight = 45;
    private float BallZ = 0;
    private float BallX = 0;
    private float xSpeed = 6;
    private float zSpeed = 6;
    private int PaddleFrenteX = 0;
    private int PaddleFundoX = 0;
    private int somaBall = 1;
    private int somaPaddleFrente = 1;
    private int somaPaddleFundo = 1;
    private Cubo Mesa;
    private Cubo CanaletaEsquerda;
    private Cubo CanaletaDireita;
    private Paddle PaddleFrente;
    private Paddle PaddleFundo;
    private Esfera Bola;
    private int texture;
    private int pontoFundo = 0;
    private int pontoFrente = 0;
    private OpenTK.Color cor = OpenTK.Color.White;
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      // Enable Light 0 and set its parameters.
      // GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 2.0f, 0.0f });
      // GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.3f, 0.3f, 0.3f, 1.0f });
      // GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      // GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      // GL.Light(LightName.Light0, LightParameter.SpotExponent, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      // GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f });
      // GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
      // GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);

      // // Use GL.Material to set your object's material parameters.
      // GL.Material(MaterialFace.Front, MaterialParameter.Ambient, new float[] { 0.3f, 0.3f, 0.3f, 1.0f });
      // GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      // GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      // GL.Material(MaterialFace.Front, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });

      // GL.Material(MaterialFace.Front, MaterialParameter.ColorIndexes, cor);

      Mesa = new Cubo("Mesa", null);
      objetosLista.Add(Mesa);
      Mesa.EscalaXYZBBox(300, 20, 600);

      Bola = new Esfera("Bola", null);
      objetosLista.Add(Bola);
      Bola.TranslacaoXYZ(0, 40, 0);

      CanaletaEsquerda = new Cubo("Canaleta Esquerda", null);
      objetosLista.Add(CanaletaEsquerda);
      CanaletaEsquerda.TranslacaoXYZ(-285, 40, 0);
      CanaletaEsquerda.EscalaXYZBBox(15, 20, 600);

      CanaletaDireita = new Cubo("Canaleta Direita", null);
      objetosLista.Add(CanaletaDireita);
      CanaletaDireita.TranslacaoXYZ(285, 40, 0);
      CanaletaDireita.EscalaXYZBBox(15, 20, 600);

      PaddleFrente = new Paddle("Paddle Frente", null);
      objetosLista.Add(PaddleFrente);
      PaddleFrente.TranslacaoXYZ(0, 30, 585);
      PaddleFrente.EscalaXYZBBox(40, 12, 8);

      PaddleFundo = new Paddle("Paddle Fundo", null);
      objetosLista.Add(PaddleFundo);
      PaddleFundo.TranslacaoXYZ(0, 30, -585);
      PaddleFundo.EscalaXYZBBox(40, 12, 8);

      camera.At = new Vector3(0, 0, 0);
      camera.Eye = new Vector3(0, 500, 1300);
      camera.Near = 100.0f;
      camera.Far = 2000.0f;

      GL.ClearColor(OpenTK.Color.WhiteSmoke);
      GL.Enable(EnableCap.DepthTest);
      //GL.Enable(EnableCap.CullFace);
      //GL.Disable(EnableCap.CullFace);
    }

    private void MoveBall() {
      
      BallX += xSpeed;
      BallZ += zSpeed;

      int ballXMax = 240; 
      int ballXMin = -240; 

      int ballZMax = 570; 
      int ballZMin = -570;

      // Console.WriteLine("BallX: " + BallX + " BallZ: " + BallZ + " |Paddle Frente Left: " + PaddleFrenteXLeft + " Paddle Frente Right: " + PaddleFrenteXRight + " |Paddle Fundo Left: " + PaddleFundoXLeft + " Paddle Fundo Right: " + PaddleFundoXRight + "|");

      // Colisão Paddle Frente
      if (BallZ == ballZMax && ((BallX >= PaddleFrenteXLeft && BallX <= PaddleFrenteXRight) || (BallX >= PaddleFundoXLeft && BallX <= PaddleFundoXRight))) 
      {
        BallZ = ballZMax;
        zSpeed = -zSpeed;
      }

      // Colisão Paddle Fundo
      else if (BallZ == ballZMin && ((BallX >= PaddleFrenteXLeft && BallX <= PaddleFrenteXRight) || (BallX >= PaddleFundoXLeft && BallX <= PaddleFundoXRight)))
      {
        BallZ = ballZMin;
        zSpeed = -zSpeed;
      }

      // Paddle Frente Errou = Ponto Paddle Fundo
      else if (BallZ > ballZMax)
      {
        pontoFundo++;
        GameReset();
      }

      // Paddle Fundo Errou = Ponto Paddle Frente
      else if (BallZ < ballZMin)
      {
        pontoFrente++;
        GameReset();
      }

      // Colisão canaletas
      if (BallX > ballXMax) 
      {
          BallX = ballXMax;
          xSpeed = -xSpeed;
      }
      else if (BallX < ballXMin)
      {
        BallX = ballXMin;
        xSpeed = -xSpeed;
      }

      Bola.TranslacaoXYZ(xSpeed, 0, zSpeed);
    }

    // Reseta o jogo ao pontuar alguem
    private void GameReset() {
      Bola.TranslacaoXYZ(-BallX, 0, -BallZ);
      BallX = 0;
      BallZ = 0;
      zSpeed = -zSpeed;
      Console.WriteLine("Frente: " + pontoFrente);
      Console.WriteLine("Fundo: " + pontoFundo);
      Console.WriteLine("*****************************");
    }

    // Movimenta paddle da frente
    private void MovePaddleFrente() {
      PaddleFrenteX += somaPaddleFrente;
      PaddleFrenteXLeft += somaPaddleFrente;
      PaddleFrenteXRight += somaPaddleFrente;
      PaddleFrente.TranslacaoXYZ(somaPaddleFrente, 0, 0);
    }

    // Movimenta paddle do fundo
    private void MovePaddleFundo() {
      PaddleFundoX += somaPaddleFundo;
      PaddleFundoXLeft += somaPaddleFundo;
      PaddleFundoXRight += somaPaddleFundo;
      PaddleFundo.TranslacaoXYZ(somaPaddleFundo, 0, 0);
    }

    protected override void OnUnload(EventArgs e)
    {
      GL.DeleteTextures(1, ref texture);
    }

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);
      GL.LoadIdentity();
      GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

      Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(camera.Fovy, Width / (float)Height, camera.Near, camera.Far);
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadMatrix(ref projection);
      GL.ClearColor(Color.CornflowerBlue);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);

      MoveBall();

      if (Keyboard[Key.Left]) {
        somaPaddleFrente = -10;
        if (PaddleFrenteX > -220)
          MovePaddleFrente();
      } else if (Keyboard[Key.Right]) {
        somaPaddleFrente = 10;
        if (PaddleFrenteX < 220)
          MovePaddleFrente();
      }

      if (Keyboard[Key.A]) {
        somaPaddleFundo = -10;
        if (PaddleFundoX > -220)
          MovePaddleFundo();
      } else if (Keyboard[Key.D]) {
        somaPaddleFundo = 10;
        if (PaddleFundoX < 220)
          MovePaddleFundo();
      }
    }
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      Matrix4 modelview = Matrix4.LookAt(camera.Eye, camera.At, camera.Up);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadMatrix(ref modelview);

      for (var i = 0; i < objetosLista.Count; i++)
        objetosLista[i].Desenhar();
      this.SwapBuffers();
    }  
  }
  class Program
  {
    static void Main(string[] args)
    {
      Mundo window = Mundo.GetInstance(600, 600);
      window.Title = "Pong Game";
      window.Run(1.0 / 60.0);
    }
  }
}
