using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UdemyRealWordUnitTest.Web.Controllers;
using UdemyRealWordUnitTest.Web.Models;
using UdemyRealWordUnitTest.Web.Repository;
using Xunit;

namespace UdemyRealWordUnitTest.Test
{
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _controller;
        private List<Product> products;


        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController (_mockRepo.Object);
            products = new List<Product>() { new Product { Id = 1, Name = "Kalem", Price = 100, Stock = 50, Color = "Kırmızı" },
            new Product { Id = 2, Name = "Defter", Price = 200, Stock = 500, Color = "Mavi" }};
        }


        //INDEX METODUNUN TESTİ
        //2 durumu var biri mutlaka index dönmesi gerek diğer durum product list gelmesi.

        [Fact]
        public async void Index_ActionExecutes_ReturnView()
        {
            //controllerdan index metodunu cagırdık.
            var result = await _controller.Index();
            //index metodundan gelen resultun tipi viewresult mı diye test ettik.
            Assert.IsType<ViewResult>(result);
        }
        [Fact]
        public async void Index_ActionExecutes_ProductList()
        {   
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(products);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            //geriye bir viewresult dönüp dönmediğini test eder.

            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            //ısassignablefrom sayesinde generic olarak verdiğimiz tipte veri dönmesini saglıyoruz
            //product verdik ve viewresultın modelinin product dönüp dönmediğini test ettik.

            Assert.Equal<int>(2, productList.Count());
            //yukarıda 2 product eklemistik.2 datanında gelip gelmediğini kontrol ettik.
        }

        //DETAİLS METODUNUN TESTİ
        // 1)id'nin null olma durumunu, redirecttoaction dönmesini ve ındexe geri dönüp dönmediğini test ediyoruz.
        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Details(null);
            //null olma durumunu test ettigimiz icin details metoduna null gönderdik.
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            //resultın redirecttoaction tipinde olması gerek.
            Assert.Equal("Index", redirect.ActionName);
            //redirecttoactionresult döndü ama hangi sayfaya döndüğünü anlamak için ındexe döndü mü diye test ederiz.
        }
        //2)ikinci if durumunda getbyıd metoduna 0 verince null dönücek mi test ettik.
        [Fact]
        public async void Details_IdInValid_ReturnNotFound()
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(0)).ReturnsAsync(product);

            var result = await _controller.Details(0);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }

        //3)id nin doğru olması durumunda(id nin 1 olmasını örnekledik) bir product dönmesini test ettik.
        [Theory]
        [InlineData(1)]
        public async void Details_ValidId_ReturnProduct(int productId)
        {
            //yukarıda tanımladıgımız product listesinden ıdsi parametre olarak verdiğimiz productıd'e esit olan productı aldık.
            Product product = products.First(x => x.Id == productId);
            //getbyıd calısmıs kodda onu taklit ettik.
            //calısan her metot icin mocklama yapılır.
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Details(productId);

            //dönen sonuc viewresult tipinde mi test ettik.
            var viewResult = Assert.IsType<ViewResult>(result);

            //product nesnesinin gelip gelmemesine bakcaz.
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            //gelen product gönderdiğimiz product mı diye test etcez.
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);

        }
        [Fact]
        public void Create_ActionExecuted_ReturnView()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        //Create metodunun post kısmını test ediyoruz.
        
        [Fact]
        public async void CreatePOST_InValidModelState_ReturnView()
        {
            //test icin bir hata oluşturcaz.(modelstate'in valid olmama durumunu test ediyoruz.)
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir.");
            //create metodu product istiyor herhangi bir product vermek için listenin ilk elemanını verdik.
            var result = await _controller.Create(products.First());
            var viewResult = Assert.IsType<ViewResult>(result);

            //dönen view resultın dönüş tipi product olmalı onu test ediyoz.
            Assert.IsType<Product>(viewResult.Model);
        }
        [Fact]
        public async void CreatePOST_ValidModelState_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Create(products.First());
            //yönlendirme yapıyor mu test etcez daha sonra ındexe gidiyor mu diye test etcez.
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
        //model state valid iken ürün ekleme metodu (create metodu)calısıyor mu test etcez.
        [Fact]
        public async void CreatePOST_ValidModelState_CreateMethodExecuted()
        {
            Product newProduct = null;
            //ıt ıs any metoduyla içerisine herhangi bir product gelebilecegini belirttik.
            //callback create metodunu simüle ettigimiz zaman ekstra calıstıracagımız metot.
            //create metodu içerisine verilen product nesnesini callback ile gelen producta aktardık.
            //yani newproducta first productı aktardık.
            _mockRepo.Setup(repo => repo.Create(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);

            var result = await _controller.Create(products.First());

            //verify ile create metodunun çalışıp/çalışmadıgını doğruladık.
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Once);
            //product listesinin ilk elemanı eklenmiş mi onu da test etmek için ilk elemanın idsi ile bizim newproductımızın idsini kıyasladık.
            Assert.Equal(products.First().Id, newProduct.Id);
        }
        //post kısmında modelstate'i eksik girilmesi durumunda create metodunun calısmaması gerek bunu test etcez.

        [Fact]
        public async void CreatePOST_InValidModelState_NeverCreateExecute()
        {
            //hata oluşturucaz.
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir.");
            var result = await _controller.Create(products.First());
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);
        }

        //EDİT METODU 
        //id null ise indexe gidicek mi test etcez.
        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Edit(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        //id olarak olmayan bir id verme durumunda product null olup not found dönecek mi test edecegiz.
        [Theory]
        [InlineData(3)]
        public async void Edit_IdInValid_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var redirect = Assert.IsType<NotFoundResult>(result);
            Assert.Equal<int>(404, redirect.StatusCode);
        }
        //ürünü olan bir id verince product dönüp/dönmeme durumu
        [Theory]
        [InlineData(2)]

        public async void Edit_ActionExecutes_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);
            //getbyıd ye productıd verdiğimizde product dönsün
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Edit(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }
        //Edit POST
        //id ler farklıysa not found dönme durumu
        [Theory]
        [InlineData(1)]
        public void EditPOST_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(2, products.First(x => x.Id == productId));
            var redirect = Assert.IsType<NotFoundResult>(result);
        }
        //modelstate hatalıysa (bir alan boşsa) edit sayfasına dönüp,productı modelde gönderme durumunu test edicez.

       [Theory]
       [InlineData(1)]
       public void EditPOST_InvalidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "Name alanı boş geçilemez.");
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));
            var viewResult = Assert.IsType<ViewResult>(result);
            //gelen datanın product nesnesi olup olmadıgını test etcez.
            Assert.IsType<Product>(viewResult.Model);
        }

        //modelstate gecerliyken index sayfasına dönüp/dönmeme durumu
        [Theory]
        [InlineData(1)]
        public void EditPOST_ValidModelState_ReturnRedirectToIndexAction(int productId)
        {
            var result = _controller.Edit(productId, products.First(x => x.Id == productId));
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        //modelstate valid iken update metodu calısıcak mı test etcez.
        [Theory]
        [InlineData(1)]
        public void EditPOST_ValidModelState_UpdateMethodExecute(int productId)
        {

            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Update(product));
            _controller.Edit(productId, product);
            //edit metodu calıstıgı zaman update in calısma durumunu doğrularız.
            _mockRepo.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
        }
        //DELETE Metodu testi
        //id null ise notfound gelmesi
        [Fact]
        public async void Delete_IdIsNull_ReturnNotFound()
        {
            //id nin null olmasını test ettigimizden null verdik.
            var result = await _controller.Delete(null);
            Assert.IsType<NotFoundResult>(result);

        }
        //id ye sahip ürün  olmama durumu ve not found dönme durumunu test etcez.
        [Theory]
        [InlineData(0)]
        public async void Delete_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(x => x.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
             Assert.IsType<NotFoundResult>(result);
        }
        //id ye sahip ürün varsa product modeli dönme durumunu test etcez.
        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecute_ReturnProduct(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);
            var result = await _controller.Delete(productId);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Product>(viewResult.Model);

        }

        //DeleteConfirmed metodu 
        //Basarılı calısırsa ındexe gidip gitmeme durumu
        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_ReturnRedirectToIndexAction(int productId)
        {
            var result = await _controller.DeleteConfirmed(productId);
            Assert.IsType<RedirectToActionResult>(result);
        }

        //Delete metodunun calısma durumunu test etcez.
        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecutes(int productId)
        {
            var product = products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Delete(product));
            await _controller.DeleteConfirmed(productId);
            _mockRepo.Verify(repo => repo.Delete(It.IsAny<Product>()), Times.Once);
        }
    }

}
