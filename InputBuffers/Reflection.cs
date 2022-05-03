using System.Reflection;

namespace InputBuffers
{
    internal class Reflection
    {
        public static readonly FieldInfo airDashed = typeof(HeroController).GetField("airDashed", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo dashCooldownTimer = typeof(HeroController).GetField("dashCooldownTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo attack_cooldown = typeof(HeroController).GetField("attack_cooldown", BindingFlags.Instance | BindingFlags.NonPublic);

        public static readonly MethodInfo CanWallJump = typeof(HeroController).GetMethod("CanWallJump", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo DoWallJump = typeof(HeroController).GetMethod("DoWallJump", BindingFlags.Instance | BindingFlags.NonPublic);
        
        public static readonly MethodInfo CanJump = typeof(HeroController).GetMethod("CanJump", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo HeroJump = typeof(HeroController).GetMethod("HeroJump", BindingFlags.Instance | BindingFlags.NonPublic);
        
        public static readonly MethodInfo CanDoubleJump = typeof(HeroController).GetMethod("CanDoubleJump", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo DoDoubleJump = typeof(HeroController).GetMethod("DoDoubleJump", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo doubleJumpQueuing = typeof(HeroController).GetField("doubleJumpQueuing", BindingFlags.Instance | BindingFlags.NonPublic);

        public static readonly MethodInfo CanDash = typeof(HeroController).GetMethod("CanDash", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo HeroDash = typeof(HeroController).GetMethod("HeroDash", BindingFlags.Instance | BindingFlags.NonPublic);

        public static readonly MethodInfo CanAttack = typeof(HeroController).GetMethod("CanAttack", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo DoAttack = typeof(HeroController).GetMethod("DoAttack", BindingFlags.Instance | BindingFlags.NonPublic);
    }
}
