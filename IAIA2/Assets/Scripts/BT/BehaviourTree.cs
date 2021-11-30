using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region "definicion de las clases abstractas que intervienen en el BT"
public abstract class BehaviorTree
{
    public List<BehaviorTree> Children { get; set; }
    public BehaviorTree Value { get; set; }
    public Grid grid { get; set; }
    protected BehaviorTree(Grid grid)
    {
        this.grid = grid;
    }
    public abstract bool Exec();
}
public abstract class Primitive : BehaviorTree
{
    protected Primitive(Grid grid) : base(grid) { }

}
public class Action : Primitive
{
    public delegate bool Act();
    public Act Function { get; set; }
    public Action(Grid grid) : base(grid)
    {
    }
    public override bool Exec()
    {
        return Function();
    }
    //public delegate bool Prueba();
    //public bool Function2 { get; set;}
}
public class Conditional : Primitive
{
    public delegate bool Pred();
    public Pred Predicate { get; set; }
    public Conditional(Grid grid)
     : base(grid)
    {
    }
    public override bool Exec()
    {
        return Predicate();
    }
}
public abstract class Composite : BehaviorTree
{
    protected Composite(Grid grid) : base(grid)
    {
    }
}
public class Sequence : Composite
{
    public Sequence(Grid grid)
     : base(grid)
    {
    }
    public List<int> Order { get; set; }
    public override bool Exec()
    {
        if (Order.Count != Children.Count)
            throw new System.Exception("Order and children count must be the same");
        foreach (var i in Order)
        {
            if (!Children[i].Exec())
                return false;
        }
        return true;
    }
}
public class Selector : Composite
{
    public Selector(Grid grid)
     : base(grid)
    {
    }
    public int Selection { get; set; }
    public override bool Exec()
    {
        return Children[Selection].Exec();
    }
}

#endregion

#region "Definicion del tablero de juego"

//Pasarle la clase grid para usar los datos necesarios como a continuacion:

/*public class GridUtilizado
{
    public int Height { get; set; }
    public int Width { get; set; }
    public int ScoreTeamA { get; set; }
    public int ScoreTeamB { get; set; }
    private readonly string[,] _grid;
    public GridUtilizado(int h, int w)
    {
        Height = h;
        Width = w;
        _grid = new string[h, w];
    }

    //...
}
*/

#endregion