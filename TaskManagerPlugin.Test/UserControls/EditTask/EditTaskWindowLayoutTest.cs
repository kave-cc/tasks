using JetBrains.DataFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using TaskManagerPlugin.Model;
using TaskManagerPlugin.Repository;
using TaskManagerPlugin.UserControls;
using TaskManagerPlugin.UserControls.EditTask;

namespace TaskManagerPlugin.Test.UserControls.EditTask
{
    [RequiresSTA]
    public class EditTaskWindowLayoutTest
    {
        private EditTaskWindowLayout _window;
        private const string FileUri = "test.json";

        [TestInitialize]
        public void SetUp()
        {
            var repositoryMock = new Mock<TaskRepository>();
            repositoryMock.Setup(repo => repo.AddTask(It.IsAny<Task>())).Returns(1L);
            var repository = repositoryMock.Object;

            var lifetime = Lifetimes.Define("Lifetime.test").Lifetime;
            var viewModel = new TaskViewModel(repository, lifetime);
            _window = new EditTaskWindowLayout(viewModel);
        }

        [TestMethod]
        public void WhenSaveClick_WindowShouldClose()
        {
            new EditTaskWindowLayout().
        }
    }
}