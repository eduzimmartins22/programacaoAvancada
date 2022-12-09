using System;
using System.Collections.Generic;
using System.Text;


abstract class AB_Universo
{
    public abstract void MoveBody(Body corpoA, Body corpoB); //Interação entre dois corpos
    public abstract void GravitationalForceBodies(Body corpoA, Body corpoB); //Calculo de distancia entre corpos
    public abstract void ForceCalculate(Body body); //Cálculo de força gravitacional
    public abstract void ClearForce(List<Body> corpos); //Limpar a força
    public abstract void InteractionForceBodies(List<Body> corpos); //Cálculo de interações de força
    public abstract void InteractionBodies(List<Body> corpos); //Cálculo das interações de gravidade
    public abstract bool WasColision(Body corpoA, Body corpoB); //Se há colisão entre algum dos corpos
    public abstract List<Body> CheckColision(List<Body> bodyes); //Verifica colisão de todos os corpos

}