using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class Universo : AB_Universo
{
    double G = 6.6740184 * Math.Pow(10, -11);

    private int qtdCorpos;
    private int qtdInteracoes;

    private int time;

    public Universo() { }

    public List<Body> ReadBodies()
    {
        List<Body> celestialBodies = new List<Body>();
        string file = "Corpos.txt";

        if (!File.Exists(file))
        {
            Console.WriteLine(String.Format("Arquivo {0} não existe", file));
            return celestialBodies;
        }

        string[] lines = File.ReadAllLines(file);
        int counter = 0;
        foreach (var line in lines)
        {
            if (counter == 0)
            {
                string[] data = line.Split(";");

                qtdCorpos = int.Parse(data[0]);
                qtdInteracoes = int.Parse(data[1]);
                time = int.Parse(data[1]);
            }

            if (counter > 0 && counter <= qtdCorpos)
            {
                Body newCelestialBody = new Body(line);
                celestialBodies.Add(newCelestialBody);
            }

            counter++;
        }

        return celestialBodies;
    }

    public List<string> WriteIterationBodies(List<string> output, List<Body> bodies)
    {
        foreach (var body in bodies)
        {
            output.Add(body.formatOutputFile());
        }

        return output;
    }

    public int getInterations()
    {
        return this.qtdInteracoes;
    }
    public double CalculateEuclidienneDistance(Body bodyA, Body bodyB)
    {
        return Math.Sqrt(Math.Pow((bodyA.getPosX() - bodyB.getPosX()), 2) + Math.Pow((bodyA.getPosY() - bodyB.getPosY()), 2));
    }


    public override void GravitationalForceBodies(Body bodyA, Body bodyB)
    {

        double r = CalculateEuclidienneDistance(bodyA, bodyB);
        double F = (G * bodyA.getMass() * bodyB.getMass()) / Math.Pow(r, 2);

        double rx = (bodyA.getPosX() - bodyB.getPosX());
        double ry = (bodyA.getPosY() - bodyB.getPosY());


        double Fx = F * (rx / r);
        double Fy = F * (ry / r);

        bodyA.setF(bodyA.getF() + (F * (-1)));
        bodyA.setFx(bodyA.getFx() + (Fx * (-1)));
        bodyA.setFy(bodyA.getFy() + (Fy * (-1)));

        bodyB.setF(bodyB.getF() + (F * (1)));
        bodyB.setFx(bodyB.getFx() + (Fx * (1)));
        bodyB.setFy(bodyB.getFy() + (Fy * (1)));
    }


    public override void MoveBody(Body bodyA, Body bodyB)
    {
        double distance = CalculateEuclidienneDistance(bodyA, bodyB);

        if (distance < (bodyA.getRadius() + bodyB.getRadius()))
        {
            double body_1_Vx = bodyA.getVelX();
            double body_1_Vy = bodyA.getVelY();

            double body_2_Vx = bodyA.getVelX();
            double body_2_Vy = bodyA.getVelY();

            bodyA.setVelX(body_2_Vx * (-1));
            bodyA.setVelY(body_2_Vy * (-1));

            bodyB.setVelX(body_1_Vx * (-1));
            bodyB.setVelY(body_1_Vy * (-1));

            ForceCalculate(bodyA);
            ForceCalculate(bodyB);
        }
    }


    public override void ForceCalculate(Body body)
    {
        double Ax = body.getFx() / body.getMass();
        double Ay = body.getFy() / body.getMass();

        double Vx = body.getVelX() + (Ax * time);
        double Vy = body.getVelY() + (Ay * time);

        double Sx = body.getPosX() + (body.getVelX() * time) + ((Ax / 2) * Math.Pow(time, 2));
        double Sy = body.getPosY() + (body.getVelY() * time) + ((Ay / 2) * Math.Pow(time, 2));

        body.setPosX(Sx);
        body.setPosY(Sy);
        body.setVelX(Vx);
        body.setVelY(Vy);
    }


    public override void ClearForce(List<Body> bodies)
    {
        foreach (var body in bodies)
        {
            body.setF(0.0f);
            body.setFx(0.0f);
            body.setFy(0.0f);
        }
    }


    public override void InteractionForceBodies(List<Body> bodies)
    {
        foreach (var body in bodies)
        {
            ForceCalculate(body);
        }
    }

    public override void InteractionBodies(List<Body> bodies)
    {

        for (var i = 0; i < bodies.Count; ++i)
        {
            for (var j = i + 1; j < bodies.Count; ++j)
            {
                MoveBody(bodies[i], bodies[j]);
            }
        }
    }

    public override bool WasColision(Body corpo_1, Body corpo_2)
    {
        double distance = corpo_1.getDistance(corpo_2);

        if (distance < (corpo_1.getRadius() + corpo_2.getRadius()))
            return true;

        return false;
    }


    List<Body> corpos_removidos = new List<Body>();
    public override List<Body> CheckColision(List<Body> corpos)
    {
        
        foreach (Body corpo in corpos)
        {
            foreach (Body corpo2 in corpos)
            {
                if (corpo == corpo2) continue;

                if (WasColision(corpo, corpo2))
                {
                    if (corpo.getRadius() > corpo2.getRadius())
                        corpos_removidos.Add(corpo2);
                    else
                        corpos_removidos.Add(corpo);
                }
            }
        }

        return corpos_removidos;
    }
}