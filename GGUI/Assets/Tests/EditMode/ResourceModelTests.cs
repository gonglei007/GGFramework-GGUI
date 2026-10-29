using NUnit.Framework;
using UnityEngine;

namespace GGUI.Tests.EditMode
{
    /// <summary>
    /// 资源层数据模型（ResourceItem / ResourceObject）与 ResourceSystem.Tag 枚举的 EditMode 单元测试。
    /// 均为纯托管数据 / 枚举，无需场景或运行时，用于守护升级后构造语义与默认值不回归。
    /// </summary>
    public class ResourceModelTests
    {
        [Test]
        public void ResourceItem_Ctor_AssignsFieldsAndDefaults()
        {
            var item = new ResourceItem("login", ResourceSystem.Tag.UI, "assets/ui/login.prefab", 5);

            Assert.AreEqual("login", item.name);
            Assert.AreEqual(ResourceSystem.Tag.UI, item.tag);
            Assert.AreEqual("assets/ui/login.prefab", item.path);
            Assert.AreEqual(5, item.priority);
            Assert.IsNull(item.prefab, "prefab 应默认为 null（尚未加载）");
            Assert.IsFalse(item.loading, "loading 应默认为 false");
            Assert.IsNull(item.onLoaded, "onLoaded 回调应默认为 null");
        }

        [Test]
        public void ResourceItem_PrioritySemantics_ArePreserved()
        {
            // priority 约定：0 初始资源；-1 不加载；>0 加载顺序。构造应原样保留该值。
            var doNotLoad = new ResourceItem("a", ResourceSystem.Tag.Other, "p", -1);
            var initial = new ResourceItem("b", ResourceSystem.Tag.Other, "p", 0);
            var ordered = new ResourceItem("c", ResourceSystem.Tag.Other, "p", 3);

            Assert.AreEqual(-1, doNotLoad.priority);
            Assert.AreEqual(0, initial.priority);
            Assert.AreEqual(3, ordered.priority);
        }

        [Test]
        public void ResourceSystemTag_EnumMembers_AreStable()
        {
            // 保护性断言：Tag 枚举成员集合与顺序稳定，防止无意重排影响加载分类。
            var names = System.Enum.GetNames(typeof(ResourceSystem.Tag));
            CollectionAssert.AreEqual(new[] { "UI", "Model", "VFX", "Other" }, names);
        }

        [Test]
        public void ResourceObject_Ctor_AssignsFields()
        {
            var texture = new Texture2D(1, 1);
            try
            {
                var obj = new ResourceObject("ui_bundle", "login", texture);

                Assert.AreEqual("ui_bundle", obj.bundleName);
                Assert.AreEqual("login", obj.assetName);
                Assert.AreSame(texture, obj.assetObject);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void ResourceObject_Ctor_AllowsNullAsset()
        {
            var obj = new ResourceObject("ui_bundle", "login", null);

            Assert.AreEqual("ui_bundle", obj.bundleName);
            Assert.AreEqual("login", obj.assetName);
            Assert.IsNull(obj.assetObject);
        }
    }
}
