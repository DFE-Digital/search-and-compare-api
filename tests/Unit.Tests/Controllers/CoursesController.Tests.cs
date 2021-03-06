// using System.Collections.Generic;
// using System.Linq;
// using GovUk.Education.SearchAndCompare.Api.Controllers;
// using GovUk.Education.SearchAndCompare.Api.DatabaseAccess;
// using GovUk.Education.SearchAndCompare.Domain.Filters;
// using GovUk.Education.SearchAndCompare.Domain.Models;
// using GovUk.Education.SearchAndCompare.Domain.Models.Joins;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.ViewFeatures;
// using MockQueryable.Moq;
// using Moq;
// using NUnit.Framework;

// namespace GovUk.Education.SearchAndCompare.Api.Unit.Tests.Controllers
// {
//     [TestFixture]
//     public class CoursesControllerTests
//     {
//         [TestFixture]
//         public class GetByIdTests
//         {
//         }

//         [TestFixture]
//         public class GetFilteredTests
//         {
//             private Subject _subject1;
//             private Subject _subject2;
//             private Subject _subject3;
//             private Subject _subject4;
//             private List<Subject> _subjects;
//             private List<CourseSubject> _courseSubjects12;
//             private List<CourseSubject> _courseSubjects34;
//             private List<Course> _courses4;
//             private List<Course> _courses14;

//             [SetUp]
//             public void SetUp()
//             {
//                 _subject1 = new Subject() { Id = 1 };
//                 _subject2 = new Subject() { Id = 2 };
//                 _subject3 = new Subject() { Id = 3 };
//                 _subject4 = new Subject() { Id = 4 };
//                 _subjects = new List<Subject>() 
//                 {
//                     _subject1, _subject2, _subject3, _subject4
//                 };
//                 _courseSubjects12 = new List<CourseSubject>()
//                 {
//                     new CourseSubject() { Subject = _subject1 },
//                     new CourseSubject() { Subject = _subject2 },
//                 };
//                 _courseSubjects34 = new List<CourseSubject>()
//                 {
//                     new CourseSubject() { Subject = _subject3 },
//                     new CourseSubject() { Subject = _subject4 },
//                 };

//                 _courses4 = new List<Course>()
//                 {
//                     new Course() { Id = 1, CourseSubjects = _courseSubjects12, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 2, CourseSubjects = _courseSubjects12, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 3, CourseSubjects = _courseSubjects12, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 4, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                 };

//                 _courses14 = new List<Course>()
//                 {
//                     new Course() { Id = 1, CourseSubjects = _courseSubjects12, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 2, CourseSubjects = _courseSubjects12, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 3, CourseSubjects = _courseSubjects12, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 4, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 5, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 6, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 7, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 8, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 9, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 10, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 11, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 12, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 13, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                     new Course() { Id = 14, CourseSubjects = _courseSubjects34, Provider = new Provider { Name = "" } },
//                 };
//             }

//             private Mock<ICourseDbContext> GetMockContext(List<Course> courses, List<Subject> subjects)
//             {
//                 var mockCourses = courses.AsQueryable().BuildMock();
//                 var mockSubjects = subjects.AsQueryable().BuildMock();

//                 var mockContext = new Mock<ICourseDbContext>();
//                 mockContext.Setup(context => context.GetCoursesWithProviderSubjectsRouteAndCampuses()).Returns(mockCourses.Object);
//                 mockContext.Setup(context => context.GetSubjects()).Returns(mockSubjects.Object);

//                 return mockContext;
//             }

//             [Test]
//             public void GivenOnePageWorthOfCourses_WhenCalledWithNullPageIndex_ThenViewModelHasFirstPageOfResults()
//             {
//                 var mockContext = GetMockContext(_courses4, _subjects);
//                 var controller = new CoursesController(mockContext.Object);

//                 var result = controller.GetFiltered(new QueryFilter()) as ViewResult;
//                 ViewDataDictionary viewData = result.ViewData;
//                 var resultsViewModel = (ResultsViewModel) result.Model;

//                 Assert.That(resultsViewModel.Courses.PageIndex, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.FilterModel.page, Is.Null);
//                 Assert.That(resultsViewModel.Courses.TotalPages, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.Courses.TotalCount, Is.EqualTo(_courses4.Count()));
//                 Assert.That(resultsViewModel.Courses, Is.EquivalentTo(_courses4));

//                 Assert.That(resultsViewModel.Subjects.TotalCount, Is.EqualTo(_subjects.Count()));
//                 Assert.That(resultsViewModel.Subjects, Is.EquivalentTo(_subjects));
//             }

//             [Test]
//             public void GivenOnePageWorthOfCourses_WhenCalledWithPageIndex3_ThenViewModelHasFirstPageOfResults()
//             {
//                 var mockContext = GetMockContext(_courses4, _subjects);
//                 var controller = new CoursesController(mockContext.Object);

//                 var result = controller.GetFiltered(new QueryFilter{page = 3}) as ViewResult;
//                 ViewDataDictionary viewData = result.ViewData;
//                 var resultsViewModel = (ResultsViewModel) result.Model;

//                 Assert.That(resultsViewModel.Courses.PageIndex, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.FilterModel.page, Is.EqualTo(3));
//                 Assert.That(resultsViewModel.Courses.TotalPages, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.Courses.TotalCount, Is.EqualTo(_courses4.Count()));
//                 Assert.That(resultsViewModel.Courses, Is.EquivalentTo(_courses4));
//             }

//             [Test]
//             public void GivenTwoPagesWorthOfCourses_WhenCalledWithNullPageIndex_ThenViewModelHasFirstPageOfResults()
//             {
//                 var mockContext = GetMockContext(_courses14, _subjects);
//                 var controller = new CoursesController(mockContext.Object);

//                 var result = controller.GetFiltered(new QueryFilter()) as ViewResult;
//                 ViewDataDictionary viewData = result.ViewData;
//                 var resultsViewModel = (ResultsViewModel) result.Model;

//                 Assert.That(resultsViewModel.Courses.PageIndex, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.FilterModel.page, Is.Null);
//                 Assert.That(resultsViewModel.Courses.TotalPages, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.Courses.TotalCount, Is.EqualTo(_courses14.Count()));
//                 Assert.That(resultsViewModel.Courses, Is.EquivalentTo(_courses14.Take(10)));
//             }

//             [Test]
//             public void GivenTwoPagesWorthOfCourses_WhenCalledWithPageIndex0_ThenViewModelHasFirstPageOfResults()
//             {
//                 var mockContext = GetMockContext(_courses14, _subjects);
//                 var controller = new CoursesController(mockContext.Object);

//                 var result = controller.GetFiltered(new QueryFilter{page = 0}) as ViewResult;
//                 ViewDataDictionary viewData = result.ViewData;
//                 var resultsViewModel = (ResultsViewModel) result.Model;

//                 Assert.That(resultsViewModel.Courses.PageIndex, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.FilterModel.page, Is.EqualTo(0));
//                 Assert.That(resultsViewModel.Courses.TotalPages, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.Courses.TotalCount, Is.EqualTo(_courses14.Count()));
//                 Assert.That(resultsViewModel.Courses, Is.EquivalentTo(_courses14.Take(10)));
//             }

//             [Test]
//             public void GivenTwoPagesWorthOfCourses_WhenCalledWithPageIndex1_ThenViewModelHasFirstPageOfResults()
//             {
//                 var mockContext = GetMockContext(_courses14, _subjects);
//                 var controller = new CoursesController(mockContext.Object);

//                 var result = controller.GetFiltered(new QueryFilter{page = 1}) as ViewResult;
//                 ViewDataDictionary viewData = result.ViewData;
//                 var resultsViewModel = (ResultsViewModel) result.Model;

//                 Assert.That(resultsViewModel.Courses.PageIndex, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.FilterModel.page, Is.EqualTo(1));
//                 Assert.That(resultsViewModel.Courses.TotalPages, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.Courses.TotalCount, Is.EqualTo(_courses14.Count()));
//                 Assert.That(resultsViewModel.Courses, Is.EquivalentTo(_courses14.Take(10)));
//             }

//             [Test]
//             public void GivenTwoPagesWorthOfCourses_WhenCalledWithPageIndex2_ThenViewModelHasSecondPageOfResults()
//             {
//                 var mockContext = GetMockContext(_courses14, _subjects);
//                 var controller = new CoursesController(mockContext.Object);

//                 var result = controller.GetFiltered(new QueryFilter{page = 2}) as ViewResult;
//                 ViewDataDictionary viewData = result.ViewData;
//                 var resultsViewModel = (ResultsViewModel) result.Model;

//                 Assert.That(resultsViewModel.Courses.PageIndex, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.FilterModel.page, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.Courses.TotalPages, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.Courses.TotalCount, Is.EqualTo(_courses14.Count()));
//                 Assert.That(resultsViewModel.Courses, Is.EquivalentTo(_courses14.Skip(10).Take(10)));
//             }

//             [Test]
//             public void GivenTwoPagesWorthOfCourses_WhenCalledWithPageIndex3_ThenViewModelHasSecondPageOfResults()
//             {
//                 var mockContext = GetMockContext(_courses14, _subjects);
//                 var controller = new CoursesController(mockContext.Object);

//                 var result = controller.GetFiltered(new QueryFilter{page = 3}) as ViewResult;
//                 ViewDataDictionary viewData = result.ViewData;
//                 var resultsViewModel = (ResultsViewModel) result.Model;

//                 Assert.That(resultsViewModel.Courses.PageIndex, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.FilterModel.page, Is.EqualTo(3));
//                 Assert.That(resultsViewModel.Courses.TotalPages, Is.EqualTo(2));
//                 Assert.That(resultsViewModel.Courses.TotalCount, Is.EqualTo(_courses14.Count()));
//                 Assert.That(resultsViewModel.Courses, Is.EquivalentTo(_courses14.Skip(10).Take(10)));
//             }
//         }
//     }
// }