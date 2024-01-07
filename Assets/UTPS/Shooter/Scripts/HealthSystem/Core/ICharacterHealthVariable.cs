public interface ICharacterHealthVariable : IHealthVariable
{
    public CharacterHealthComponent.HitBox[] HitBoxes { get; }
}