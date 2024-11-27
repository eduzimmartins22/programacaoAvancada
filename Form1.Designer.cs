using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Classe concreta que implementa todos os métodos abstratos de AB_Universo
class Universo : AB_Universo
{
    private double G = 6.6740184 * Math.Pow(10, -11); // Constante gravitacional
    private int qtdCorpos; // Quantidade de corpos
    private int qtdInteracoes; // Quantidade de iterações
    private double tempo; // Intervalo de tempo para os cálculos
    private List<Body> corpos = new List<Body>(); // Lista de corpos
    private List<Body> valoresIniciaisCorpos = new List<Body>(); // Estados iniciais dos corpos
    private Random rnd = new Random(); // Gerador de números aleatórios

    // Construtor do Universo
    public Universo(int qtdCorpos, double intervaloTempo)
    {
        this.qtdCorpos = qtdCorpos;
        this.qtdInteracoes = 0;
        this.tempo = intervaloTempo;
    }

    // Recupera a lista de corpos
    public List<Body> GetCorpos() => corpos;

    // Recupera a quantidade de interações
    public int GetQtdInteracoes() => qtdInteracoes;

    // Configura a quantidade de interações
    public void SetQtdInteracoes(int qtdInteracoes) => this.qtdInteracoes = qtdInteracoes;

    // Salva o estado inicial dos corpos
    public void SalvarValoresIniciaisCorpos()
    {
        valoresIniciaisCorpos = corpos.Select(corpo => corpo.Clonar()).ToList();
    }

    // Lê corpos de um arquivo (exemplo: Corpos.txt)
    public List<Body> ReadBodies()
    {
        List<Body> celestialBodies = new List<Body>();
        string file = "Corpos.txt";

        if (!File.Exists(file))
        {
            Console.WriteLine($"Arquivo {file} não existe.");
            return celestialBodies;
        }

        string[] lines = File.ReadAllLines(file);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue; // Ignora linhas vazias
            try
            {
                Body newCelestialBody = new Body(line); // Tenta criar um corpo
                celestialBodies.Add(newCelestialBody);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Erro ao processar a linha: {line}. Detalhes: {ex.Message}");
            }
        }

        corpos = celestialBodies;
        return celestialBodies;
    }

    // Gera corpos aleatórios
    public void GerarCorposAleatorios(int minMassa, int maxMassa, int minRaio, int maxRaio)
    {
        corpos.Clear();
        for (int i = 0; i < qtdCorpos; i++)
        {
            Body corpo = new Body();
            corpo.setName($"Corpo_{i + 1}");
            corpo.setMass(rnd.Next(minMassa, maxMassa));
            corpo.setRadius(rnd.Next(minRaio, maxRaio));
            corpo.setPosX(rnd.NextDouble() * 1000);
            corpo.setPosY(rnd.NextDouble() * 1000);
            corpo.setVelX(rnd.NextDouble() * 10 - 5); // Velocidade inicial aleatória
            corpo.setVelY(rnd.NextDouble() * 10 - 5);
            corpos.Add(corpo);
        }
    }

    // Iteração gravitacional entre todos os corpos
    public override void InteractionForceBodies(List<Body> corpos)
    {
        Parallel.ForEach(corpos, corpo =>
        {
            Parallel.ForEach(corpos, outroCorpo =>
            {
                if (corpo != outroCorpo)
                {
                    double r = CalculateEuclidienneDistance(corpo, outroCorpo);
                    if (r > 0)
                    {
                        double F = (G * corpo.getMass() * outroCorpo.getMass()) / Math.Pow(r, 2);
                        double deltaX = outroCorpo.getPosX() - corpo.getPosX();
                        double deltaY = outroCorpo.getPosY() - corpo.getPosY();
                        double Fx = F * (deltaX / r);
                        double Fy = F * (deltaY / r);
                        corpo.setFx(corpo.getFx() + Fx);
                        corpo.setFy(corpo.getFy() + Fy);
                    }
                }
            });
        });
    }

    // Aplica as forças para atualizar posições e velocidades
    public void AplicaForca()
    {
        Parallel.ForEach(corpos, corpo =>
        {
            double Ax = corpo.getFx() / corpo.getMass();
            double Ay = corpo.getFy() / corpo.getMass();

            corpo.setVelX(corpo.getVelX() + Ax * tempo);
            corpo.setVelY(corpo.getVelY() + Ay * tempo);

            corpo.setPosX(corpo.getPosX() + corpo.getVelX() * tempo + 0.5 * Ax * Math.Pow(tempo, 2));
            corpo.setPosY(corpo.getPosY() + corpo.getVelY() * tempo + 0.5 * Ay * Math.Pow(tempo, 2));

            corpo.setFx(0); // Limpa a força acumulada após aplicar
            corpo.setFy(0);
        });
    }

    // Verifica colisões entre os corpos
    public void VerificaColisao()
    {
        var colisoes = new ConcurrentBag<(Body, Body)>();
        Parallel.For(0, corpos.Count, i =>
        {
            for (int j = i + 1; j < corpos.Count; j++)
            {
                double distancia = CalculateEuclidienneDistance(corpos[i], corpos[j]);
                if (distancia < corpos[i].getRadius() + corpos[j].getRadius())
                {
                    colisoes.Add((corpos[i], corpos[j]));
                }
            }
        });

        List<Body> novosCorpos = new List<Body>();
        foreach (var (corpoA, corpoB) in colisoes)
        {
            Body novoCorpo = corpoA + corpoB; // Cria um novo corpo a partir da colisão
            novosCorpos.Add(novoCorpo);
            lock (corpos)
            {
                corpos.Remove(corpoA);
                corpos.Remove(corpoB);
            }
        }

        lock (corpos)
        {
            corpos.AddRange(novosCorpos);
        }
    }

    // Calcula a distância entre dois corpos
    private double CalculateEuclidienneDistance(Body bodyA, Body bodyB)
    {
        return Math.Sqrt(Math.Pow(bodyA.getPosX() - bodyB.getPosX(), 2) + Math.Pow(bodyA.getPosY() - bodyB.getPosY(), 2));
    }

    // Implementação de métodos abstratos
    public override void GravitationalForceBodies(Body bodyA, Body bodyB)
    {
        double r = CalculateEuclidienneDistance(bodyA, bodyB);
        if (r > 0)
        {
            double F = (G * bodyA.getMass() * bodyB.getMass()) / Math.Pow(r, 2);
            double deltaX = bodyB.getPosX() - bodyA.getPosX();
            double deltaY = bodyB.getPosY() - bodyA.getPosY();
            bodyA.setFx(bodyA.getFx() + F * (deltaX / r));
            bodyA.setFy(bodyA.getFy() + F * (deltaY / r));
        }
    }
}
