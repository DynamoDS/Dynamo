using System;
using System.Linq;
using Dynamo.LibraryUI;
using Dynamo.Wpf.Interfaces;
using Moq;
using NUnit.Framework;

namespace ViewExtensionLibraryTests
{
    public class LibraryViewCustomizationTests
    {
        [Test, Category("UnitTests")]
        public void AddSections()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();
            var spec = customization.GetSpecification();
            Assert.False(spec.sections.Any()); //By default empty spec

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            var sections = new[] { "A", "B", "C" }.Select(s => new LayoutSection(s));
            Assert.True(customization.AddSections(sections));

            spec = customization.GetSpecification();
            Assert.AreEqual(3, spec.sections.Count);
            Assert.AreEqual("A, B, C", string.Join(", ", spec.sections.Select(s => s.text)));

            //Adding same set of sections returns false
            Assert.False(customization.AddSections(sections.Take(1)));
            Assert.False(customization.AddSections(sections));

            spec = customization.GetSpecification();
            Assert.AreEqual(3, spec.sections.Count);
            Assert.AreEqual("A, B, C", string.Join(", ", spec.sections.Select(s => s.text)));

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Once); //Only notified once
        }

        [Test, Category("UnitTests")]
        public void AddElementsToDefaultSection()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            var elements = new[] { "A", "B", "C" }.Select(s => new LayoutElement(s) { elementType = LayoutElementType.category });
            Assert.True(customization.AddElements(elements));

            var spec = customization.GetSpecification();
            var section = spec.sections.FirstOrDefault();
            Assert.AreEqual(1, spec.sections.Count);
            Assert.AreEqual(LibraryViewCustomization.DefaultSectionName, section.text);

            Assert.AreEqual("A, B, C", string.Join(", ", section.childElements.Select(s => s.text)));

            //Adding same set of elements to the given section should throw exception
            Assert.Throws<InvalidOperationException>(() => customization.AddElements(elements.Take(1)));
            Assert.Throws<InvalidOperationException>(() => customization.AddElements(elements));

            spec = customization.GetSpecification();
            section = spec.sections.FirstOrDefault();
            Assert.AreEqual(1, spec.sections.Count);
            Assert.AreEqual(LibraryViewCustomization.DefaultSectionName, section.text);

            Assert.AreEqual("A, B, C", string.Join(", ", section.childElements.Select(s => s.text)));

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Once); //Only notified once
        }

        [Test, Category("UnitTests")]
        public void AddElementsToNewSection()
        {
            var customization = new LibraryViewCustomization();

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            VerifyAddElements(customization, "XYZ");
            controller.Verify(c => c.RaiseEvent(eventanme), Times.Once); //Only notified once
        }

        [Test, Category("UnitTests")]
        public void AddElementsToExistingSection()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            var sectiontext = "X";
            customization.AddSections(new[] { "X", "Y", "Z" }.Select(s => new LayoutSection(s)));

            var spec = customization.GetSpecification();
            Assert.AreEqual(3, spec.sections.Count);

            VerifyAddElements(customization, sectiontext, 3);

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Exactly(2)); //Only notified twice
        }

        [Test, Category("UnitTests")]
        public void AddSameElementsToDifferentSection()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            customization.AddSections(new[] { "X", "Y", "Z" }.Select(s => new LayoutSection(s)));

            var spec = customization.GetSpecification();
            Assert.AreEqual(3, spec.sections.Count);

            VerifyAddElements(customization, "Y", 3);
            VerifyAddElements(customization, "Z", 3);

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Exactly(3)); //Only notified thrice
        }

        internal static void VerifyAddElements(ILibraryViewCustomization customization, string sectiontext, int sections = 1)
        {
            var elements = new[] { "A", "B", "C" }.Select(s => new LayoutElement(s) { elementType = LayoutElementType.category });
            Assert.True(customization.AddElements(elements, sectiontext));

            var spec = customization.GetSpecification();
            var section = spec.sections.FirstOrDefault(s => string.Equals(sectiontext, s.text));
            Assert.AreEqual(sections, spec.sections.Count);
            Assert.AreEqual(sectiontext, section.text);

            Assert.AreEqual("A, B, C", string.Join(", ", section.childElements.Select(s => s.text)));

            //Adding same set of elements should throw exception
            Assert.Throws<InvalidOperationException>(() => customization.AddElements(elements.Take(1), sectiontext));
            Assert.Throws<InvalidOperationException>(() => customization.AddElements(elements, sectiontext));

            spec = customization.GetSpecification();
            section = spec.sections.FirstOrDefault(s => string.Equals(sectiontext, s.text));
            Assert.AreEqual(sections, spec.sections.Count);
            Assert.AreEqual(sectiontext, section.text);

            Assert.AreEqual("A, B, C", string.Join(", ", section.childElements.Select(s => s.text)));
        }

        [Test, Category("UnitTests")]
        public void AddIncludeInfoToDefaultSection()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            var includes = new[] { "A", "B", "C", "D" }.Select(s => new LayoutIncludeInfo() { path = s });
            Assert.True(customization.AddIncludeInfo(includes));

            var spec = customization.GetSpecification();
            var section = spec.sections.FirstOrDefault();
            Assert.AreEqual(1, spec.sections.Count);
            Assert.AreEqual(LibraryViewCustomization.DefaultSectionName, section.text);

            Assert.AreEqual("A, B, C, D", string.Join(", ", section.include.Select(s => s.path)));

            //Adding same set of includes should throw exception
            Assert.Throws<InvalidOperationException>(() => customization.AddIncludeInfo(includes.Take(1)));
            Assert.Throws<InvalidOperationException>(() => customization.AddIncludeInfo(includes));

            spec = customization.GetSpecification();
            section = spec.sections.FirstOrDefault();
            Assert.AreEqual(1, spec.sections.Count);
            Assert.AreEqual(LibraryViewCustomization.DefaultSectionName, section.text);

            Assert.AreEqual("A, B, C, D", string.Join(", ", section.include.Select(s => s.path)));
            controller.Verify(c => c.RaiseEvent(eventanme), Times.Once); //Only notified once
        }

        [Test, Category("UnitTests")]
        public void AddIncludeInfoToNewSection()
        {
            var customization = new LibraryViewCustomization();

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            VerifyAddIncludeInfo(customization, "Xyz");

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Once); //Only notified once
        }

        [Test, Category("UnitTests")]
        public void AddIncludeInfoToExistingSection()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();
            var sectiontext = "X";
            customization.AddSections(new[] { "X", "Y", "Z" }.Select(s => new LayoutSection(s)));

            var spec = customization.GetSpecification();
            Assert.AreEqual(3, spec.sections.Count);

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            VerifyAddIncludeInfo(customization, sectiontext, 3);

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Once); //Only notified once
        }

        [Test, Category("UnitTests")]
        public void AddSameIncludeInfoToDifferentSection()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();
            customization.AddSections(new[] { "X", "Y", "Z" }.Select(s => new LayoutSection(s)));

            var spec = customization.GetSpecification();
            Assert.AreEqual(3, spec.sections.Count);

            var eventanme = "SpecChanged";
            var controller = new Mock<IEventController>();
            customization.SpecificationUpdated += (o, e) => controller.Object.RaiseEvent(eventanme);

            VerifyAddIncludeInfo(customization, "Y", 3);
            VerifyAddIncludeInfo(customization, "Z", 3);

            controller.Verify(c => c.RaiseEvent(eventanme), Times.Exactly(2)); //Only notified twice since we register the event handler
        }

        internal static void VerifyAddIncludeInfo(ILibraryViewCustomization customization, string sectiontext, int sections = 1)
        {
            var includes = new[] { "A", "B", "C", "D" }.Select(s => new LayoutIncludeInfo() { path = s });
            Assert.True(customization.AddIncludeInfo(includes, sectiontext));

            var spec = customization.GetSpecification();
            var section = spec.sections.FirstOrDefault(s => string.Equals(sectiontext, s.text));
            Assert.AreEqual(sections, spec.sections.Count);
            Assert.AreEqual(sectiontext, section.text);

            Assert.AreEqual("A, B, C, D", string.Join(", ", section.include.Select(s => s.path)));

            //Adding same set of includes should throw exception
            Assert.Throws<InvalidOperationException>(() => customization.AddIncludeInfo(includes.Take(1), sectiontext));
            Assert.Throws<InvalidOperationException>(() => customization.AddIncludeInfo(includes, sectiontext));

            spec = customization.GetSpecification();
            section = spec.sections.FirstOrDefault(s => string.Equals(sectiontext, s.text));
            Assert.AreEqual(sections, spec.sections.Count);
            Assert.AreEqual(sectiontext, section.text);

            Assert.AreEqual("A, B, C, D", string.Join(", ", section.include.Select(s => s.path)));
        }

        [Test, Category("UnitTests")]
        public void GetSpecification()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();
            var sections = new[] { "A", "B", "C" }.Select(s => new LayoutSection(s));
            customization.AddSections(sections);

            var spec1 = customization.GetSpecification();
            Assert.AreEqual(3, spec1.sections.Count);
            var spec2 = customization.GetSpecification();
            Assert.AreEqual(3, spec2.sections.Count);
            Assert.AreNotSame(spec1, spec2);

            Assert.AreEqual("A, B, C", string.Join(", ", spec1.sections.Select(s => s.text)));
            Assert.AreEqual("A, B, C", string.Join(", ", spec2.sections.Select(s => s.text)));
        }

        [Test, Category("UnitTests")]
        public void ToJSONStream()
        {
            ILibraryViewCustomization customization = new LibraryViewCustomization();
            var sections = new[] { "A", "B", "C" }.Select(s => new LayoutSection(s));
            customization.AddSections(sections);

            using(var stream = customization.ToJSONStream())
            {
                var spec = LayoutSpecification.FromJSONStream(stream);
                Assert.AreEqual(3, spec.sections.Count);
                Assert.AreEqual("A, B, C", string.Join(", ", spec.sections.Select(s => s.text)));
            }
        }
    }
}
