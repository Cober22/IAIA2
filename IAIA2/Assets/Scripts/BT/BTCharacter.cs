using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BTCharacter
{
    //Ahora mismo es una FSM, se puede adaptar para el BT haciendo algunos cambios
    public enum Percept
    {
        UnitMoneySupply, //Dinero >= 20 para alimentar a la unidad
        CastleUnderAttack, //Castillo siendo atacado
        EnemyClose, //Enemigo en rango
        WeakEnemy, //Enemigo a menos del 50% de vida
        VilleRangeToConquer, //Villa en rango cercano que se puede conquistar
        None
    }
    public enum Mode
    {
        Ataque,
        Defensa
    }
    public enum Direction
    {
        Up, Down, Left, Right
    }

    public BTCharacter()
    {

    }


    /*public class Player
    {
        public Point Position { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int Speed { get; set; }
        public double ShootingAccuracy { get; set; }
        public bool BallPossession { get; set; }
        public LinkedList<PathNode> Path;
        private readonly Court _court;
        private readonly Random _random;
        public Player(Court court, Point basket)
        {
            ScoringBasket = new Point(basket.X, basket.Y);
            _court = court;
            Path = new LinkedList<PathNode>();
            _random = new Random();
        }
    }
    */
    private IEnumerable<Percept> GetPercepts()
    {
        //Analizar situacion, añadiendo en result todos los elementos que percibe el agente
        var result = new List<Percept>();
        if (IsUnitMoneySupply())
            result.Add(Percept.UnitMoneySupply);
        if (IsCastleUnderAttack())
            result.Add(Percept.CastleUnderAttack);
        if (IsEnemyClose())
            result.Add(Percept.EnemyClose);
        if (IsWeakEnemy())
            result.Add(Percept.WeakEnemy);
        else
            result.Add(Percept.VilleRangeToConquer);
        return result;
    }

    private bool IsUnitMoneySupply()
    {
        //Se comprueba que el dinero de la IA es >= 20 para mantener viva a la unidad
        return true;
    }
    private bool IsCastleUnderAttack()
    {
        return true;
    }
    private bool IsEnemyClose()
    {
        return true;
    }
    private bool IsWeakEnemy()
    {
        return true;
    }

    public void Action()
    {
        var percepts = GetPercepts();
        if (!percepts.Contains(Percept.UnitMoneySupply))
        {
            //Muere?
        }
        else if(percepts.Contains(Percept.CastleUnderAttack))
        {
            //Modo Defensa
        }
        else if(!percepts.Contains(Percept.EnemyClose))
        {
            //Modo anterior se mantiene
        }
        else if(percepts.Contains(Percept.EnemyClose))
        {
            //Si es un guerrero ataca(), si no:
            //else if(percepts.Contains(Percept.WeakEnemy)){ //Modo Defensa}
            //else { Modo Ataque }
        }

    }


}

