using NUnit.Framework;
using UnityEngine;

namespace DuskBlade.Tests
{
    [Category("Reference"), Category("EditMode")]
    public class TestReflectionHelperEditModeTests
    {
        [Test]
        public void FindComponentByClassName_ReturnsDerivedComponentForBaseTypeName()
        {
            var root = new GameObject("ReflectionHelper_Test");
            try
            {
                root.AddComponent<SphereCollider>();
                Component found = TestReflectionHelper.FindComponentByClassName(root, nameof(Collider));
                Assert.IsNotNull(found);
                Assert.IsInstanceOf<SphereCollider>(found);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
