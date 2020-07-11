/**
  Autor: Dalton Solano dos Reis
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
    private int BallZ = 0;
    private int soma = 1;
#if CG_Privado
    //private Privado_SegReta obj_SegReta;
    //private Privado_Circulo obj_Circulo;
#endif
    private Cubo Mesa;
    private Cubo CanaletaEsquerda;
    private Cubo CanaletaDireita;
    private Paddle PaddleFrente;
    private Paddle PaddleFundo;
    private Esfera Bola;
    private int texture;
    private OpenTK.Color cor = OpenTK.Color.White;

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      // Enable Light 0 and set its parameters.
      GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 2.0f, 0.0f });
      GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.3f, 0.3f, 0.3f, 1.0f });
      GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      GL.Light(LightName.Light0, LightParameter.SpotExponent, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f });
      GL.LightModel(LightModelParameter.LightModelTwoSide, 1);
      GL.LightModel(LightModelParameter.LightModelLocalViewer, 1);

      // Use GL.Material to set your object's material parameters.
      GL.Material(MaterialFace.Front, MaterialParameter.Ambient, new float[] { 0.3f, 0.3f, 0.3f, 1.0f });
      GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
      GL.Material(MaterialFace.Front, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });

      //FIXME: cor só aparece nas superfícies laterais. Ter mais tipos de luz.      
      GL.Material(MaterialFace.Front, MaterialParameter.ColorIndexes, cor);

      Console.WriteLine(" --- Ajuda / Teclas: ");
      Console.WriteLine(" [  H     ] mostra teclas usadas. ");

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
      PaddleFrente.EscalaXYZBBox(60, 12, 8);

      PaddleFundo = new Paddle("Paddle Fundo", null);
      objetosLista.Add(PaddleFundo);
      PaddleFundo.TranslacaoXYZ(0, 30, -585);
      PaddleFundo.EscalaXYZBBox(60, 12, 8);

      objetoSelecionado = Bola;

      camera.At = new Vector3(0, 0, 0);
      camera.Eye = new Vector3(0, 500, 1300);
      camera.Near = 100.0f;
      camera.Far = 2000.0f;

      GL.ClearColor(127,127,127,255);
      GL.Enable(EnableCap.DepthTest);
      //GL.Enable(EnableCap.CullFace);
      //GL.Disable(EnableCap.CullFace);
    }

    private void MoveBall() {
      BallZ += 15 * soma;
      Bola.TranslacaoXYZ(0, 0, BallZ);
    }

    protected override void OnUnload(EventArgs e)
    {
      GL.DeleteTextures(1, ref texture);
    }

    protected override void OnResize(EventArgs e)
    {
      base.OnResize(e);

      GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

      Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(camera.Fovy, Width / (float)Height, camera.Near, camera.Far);
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadMatrix(ref projection);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      base.OnUpdateFrame(e);
      Console.WriteLine("Ballz: " + BallZ);

      if (BallZ < 120 && BallZ > -120)
        MoveBall();
      else {
        soma *= -1;
        MoveBall();
      }
    }
    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      Matrix4 modelview = Matrix4.LookAt(camera.Eye, camera.At, camera.Up);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadMatrix(ref modelview);
#if CG_Gizmo      
      //Sru3D();
#endif
      for (var i = 0; i < objetosLista.Count; i++)
        objetosLista[i].Desenhar();
      if (bBoxDesenhar && (objetoSelecionado != null))
        objetoSelecionado.BBox.Desenhar();
      this.SwapBuffers();
    }

    protected override void OnKeyDown(OpenTK.Input.KeyboardKeyEventArgs e)
    {
      if (e.Key == Key.H)
        Utilitario.AjudaTeclado();
      else if (e.Key == Key.Escape)
        Exit();
      else if (e.Key == Key.E)
      {
        Console.WriteLine("--- Objetos / Pontos: ");
        for (var i = 0; i < objetosLista.Count; i++)
        {
          Console.WriteLine(objetosLista[i]);
        }
      }
      else if (e.Key == Key.O)
        bBoxDesenhar = !bBoxDesenhar;
      else if (e.Key == Key.Enter)
      {
        if (objetoNovo != null)
        {
          objetoNovo.PontosRemoverUltimo();   // N3-Exe6: "truque" para deixar o rastro
          objetoSelecionado = objetoNovo;
          objetoNovo = null;
        }
      }
      // else if (e.Key == Key.Space)
      // {
      //   if (objetoNovo == null)
      //   {
      //     objetoNovo = new Poligono(objetoId + 1, null);
      //     objetosLista.Add(objetoNovo);
      //     objetoNovo.PontosAdicionar(new Ponto4D(mouseX, mouseY));
      //     objetoNovo.PontosAdicionar(new Ponto4D(mouseX, mouseY));  // N3-Exe6: "troque" para deixar o rastro
      //   }
      //   else
      //     objetoNovo.PontosAdicionar(new Ponto4D(mouseX, mouseY));
      // }
      else if (objetoSelecionado != null)
      {
        if (e.Key == Key.M)
          Console.WriteLine(objetoSelecionado.Matriz);
        else if (e.Key == Key.P)
          Console.WriteLine(objetoSelecionado);
        else if (e.Key == Key.I)
          objetoSelecionado.AtribuirIdentidade();
        else if (e.Key == Key.Left)
          PaddleFrente.TranslacaoXYZ(-10, 0, 0);
        else if (e.Key == Key.Right)
          PaddleFrente.TranslacaoXYZ(10, 0, 0);
        else if (e.Key == Key.A)
          PaddleFundo.TranslacaoXYZ(-10, 0, 0);
        else if (e.Key == Key.D)
          PaddleFundo.TranslacaoXYZ(10, 0, 0);
        // else if (e.Key == Key.Up)
          // objetoSelecionado.TranslacaoXYZ(0, 10, 0);
        // else if (e.Key == Key.Down)
          // objetoSelecionado.TranslacaoXYZ(0, -10, 0);
        else if (e.Key == Key.Number8)
          objetoSelecionado.TranslacaoXYZ(0, 0, 10);
        else if (e.Key == Key.Number9)
          objetoSelecionado.TranslacaoXYZ(0, 0, -10);
        else if (e.Key == Key.PageUp)
          objetoSelecionado.EscalaXYZ(2, 2, 2);
        else if (e.Key == Key.PageDown)
          objetoSelecionado.EscalaXYZ(0.5, 0.5, 0.5);
        else if (e.Key == Key.Home)
          objetoSelecionado.EscalaXYZBBox(0.5, 0.5, 0.5);
        else if (e.Key == Key.End)
          objetoSelecionado.EscalaXYZBBox(2, 2, 2);
        else if (e.Key == Key.Number1)
          objetoSelecionado.Rotacao(10);
        else if (e.Key == Key.Number2)
          objetoSelecionado.Rotacao(-10);
        else if (e.Key == Key.Number3)
          objetoSelecionado.RotacaoZBBox(10);
        else if (e.Key == Key.Number4)
          objetoSelecionado.RotacaoZBBox(-10);
        else if (e.Key == Key.Number0)
          objetoSelecionado = null;
        else if (e.Key == Key.X)
          objetoSelecionado.TrocaEixoRotacao('x');
        else if (e.Key == Key.Y)
          objetoSelecionado.TrocaEixoRotacao('y');
        else if (e.Key == Key.Z)
          objetoSelecionado.TrocaEixoRotacao('z');
        else
          Console.WriteLine(" __ Tecla não implementada.");
      }
      else
        Console.WriteLine(" __ Tecla não implementada.");
    }

    //TODO: não está considerando o NDC
    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
      mouseX = e.Position.X; mouseY = 600 - e.Position.Y; // Inverti eixo Y
      if (objetoNovo != null)
      {
        objetoNovo.PontosUltimo().X = mouseX;
        objetoNovo.PontosUltimo().Y = mouseY;
      }
    }

#if CG_Gizmo
    private void Sru3D()
    {
      GL.LineWidth(1);
      GL.Begin(PrimitiveType.Lines);
      GL.Color3(OpenTK.Color.Red);
      GL.Vertex3(0, 0, 0); GL.Vertex3(200, 0, 0);
      GL.Color3(OpenTK.Color.Green);
      GL.Vertex3(0, 0, 0); GL.Vertex3(0, 200, 0);
      GL.Color3(OpenTK.Color.Blue);
      GL.Vertex3(0, 0, 0); GL.Vertex3(0, 0, 200);
      GL.End();
    }
#endif    
  }
  class Program
  {
    static void Main(string[] args)
    {
      Mundo window = Mundo.GetInstance(600, 600);
      window.Title = "CG-N4";
      window.Run(1.0 / 60.0);
    }
  }
}
