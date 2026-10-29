using System.Collections.Generic;
using Loxodon.Framework.Bundles;
using NUnit.Framework;
using UnityEngine;

namespace GGUI.Tests.EditMode
{
    /// <summary>
    /// GMappingPathInfoParser 的 EditMode 单元测试。
    /// 该解析器基于 BundleManifest 建立「资产路径(小写) -> AssetBundle 名」映射，
    /// 仅索引 Published==true 的 bundle，Parse 大小写不敏感，未命中返回 null。
    /// 这些是纯逻辑，无需场景 / AssetDatabase / NGUI 运行时，适合 EditMode 验证升级后行为不回归。
    /// </summary>
    public class GMappingPathInfoParserTests
    {
        private static BundleInfo MakeBundle(string name, bool published, params string[] assets)
        {
            // BundleInfo 构造要求 name / filename 非空；hash 用默认 Hash128 即可， crc / fileSize 无关本测试。
            return new BundleInfo(name, string.Empty, new Hash128(), 0u, 0L, name, published, assets, null);
        }

        private static GMappingPathInfoParser MakeParser(params BundleInfo[] bundles)
        {
            var manifest = new BundleManifest(new List<BundleInfo>(bundles), "1.0.0");
            return new GMappingPathInfoParser(manifest);
        }

        [Test]
        public void Parse_PublishedAsset_ReturnsBundleNameAndQueriedPath()
        {
            var parser = MakeParser(MakeBundle("ui_bundle", true, "assets/ui/login.prefab"));

            AssetPathInfo info = parser.Parse("assets/ui/login.prefab");

            Assert.IsNotNull(info);
            Assert.AreEqual("ui_bundle", info.BundleName);
            Assert.AreEqual("assets/ui/login.prefab", info.AssetName);
        }

        [Test]
        public void Parse_IsCaseInsensitive_OnQuery()
        {
            var parser = MakeParser(MakeBundle("ui_bundle", true, "Assets/UI/Login.prefab"));

            AssetPathInfo info = parser.Parse("assets/ui/login.prefab");

            Assert.IsNotNull(info);
            Assert.AreEqual("ui_bundle", info.BundleName);
        }

        [Test]
        public void Parse_PreservesQueriedPathCasingInAssetName()
        {
            var parser = MakeParser(MakeBundle("ui_bundle", true, "assets/ui/login.prefab"));

            AssetPathInfo info = parser.Parse("Assets/UI/LOGIN.prefab");

            Assert.IsNotNull(info);
            // Parse 用传入的原始 path 构造 AssetPathInfo，映射查表用小写，但返回值应保留查询时的大小写。
            Assert.AreEqual("Assets/UI/LOGIN.prefab", info.AssetName);
        }

        [Test]
        public void Parse_UnpublishedBundleAsset_ReturnsNull()
        {
            var parser = MakeParser(MakeBundle("dep_bundle", false, "assets/data/config.bytes"));

            Assert.IsNull(parser.Parse("assets/data/config.bytes"), "未发布 bundle 的资产不应被索引，应返回 null");
        }

        [Test]
        public void Parse_UnknownPath_ReturnsNull()
        {
            var parser = MakeParser(MakeBundle("ui_bundle", true, "assets/ui/login.prefab"));

            Assert.IsNull(parser.Parse("assets/ui/does_not_exist.prefab"));
        }

        [Test]
        public void Parse_MultiplePublishedBundles_MapsEachAssetToItsOwnBundle()
        {
            var parser = MakeParser(
                MakeBundle("ui_bundle", true, "assets/ui/login.prefab"),
                MakeBundle("model_bundle", true, "assets/model/hero.fbx"));

            Assert.AreEqual("ui_bundle", parser.Parse("assets/ui/login.prefab").BundleName);
            Assert.AreEqual("model_bundle", parser.Parse("assets/model/hero.fbx").BundleName);
        }
    }
}
