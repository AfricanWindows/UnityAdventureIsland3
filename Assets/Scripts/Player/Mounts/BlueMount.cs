/// <summary>
/// ROLE: The blue animal (shoots) - an empty type so a pickup can name it.
/// PATTERNS: none.
///
/// The blue animal. Empty on purpose: everything an animal is lives in AnimalMount, and
/// this class exists only so a pickup can say WHICH animal - the same job PickableDropper
/// and StaticEnemy do for their own generic bases.
/// </summary>
public class BlueMount : AnimalMount
{
}
