using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApiTask.Models;

namespace WebApiTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResidentsController : ControllerBase
    {
        ResidentContext db; //база данных

        //Подключение к бд и сохранение результатов в MyDb.db
        public ResidentsController(ResidentContext context)
        {
            db = context;
            if (!db.Residents.Any())
            {
                db.SaveChanges();
            }
        }
        //Получение всех ЛС
        [HttpGet("/Получить все ЛС")]
        public async Task<ActionResult<IEnumerable<Resident>>> Get()
        {
            return await db.Residents.ToListAsync();
        }

        // Просмотр ЛС по id
        [HttpGet("/Просмотр по id")]
        public async Task<ActionResult<Resident>> Get(int id)
        {
            Resident resident = await db.Residents.FirstOrDefaultAsync(x => x.Id == id);
            if (resident == null)
                return NotFound();
            return new ObjectResult(resident);
        }

        //Просмотр ЛС по номеру
        [HttpGet("/Просмотр по ЛС")]
        public async Task<ActionResult<Resident>> Get(string numberLc)
        {
            Resident resident = await db.Residents.FirstOrDefaultAsync(x => x.NumberLc.Contains(numberLc));
            if (resident == null)
                return NotFound();
            return new ObjectResult(resident);
        }

        //Поиск по площади
        [HttpGet("/Поиск по площади")]
        public async Task<ActionResult<Resident>> GetByArea(int area)
        {
            Resident resident = await db.Residents.FirstOrDefaultAsync(x => x.Area == area);
            if (resident == null)
                return NotFound();
            return new ObjectResult(resident);
        }

        //Фильтр квартир по площади
        [HttpGet("/Фильтр всех квартир, превышающих указанную площадь по площади")]
        public async Task<ActionResult<IEnumerable<Resident>>> GetByAreaFilter(int area)
        {
            return await db.Residents.Where(c => c.Area > area).ToListAsync();
        }

        //Поиск по адресу
        [HttpGet("/Поиск по адресу")]
        public async Task<ActionResult<Resident>> GetByAddress(string address)
        {
            Resident resident = await db.Residents.FirstOrDefaultAsync(x => x.Address == address);
            if (resident == null)
                return NotFound();
            return new ObjectResult(resident);
        }

        //Поиск ЛС по ФИО проживающего
        [HttpGet("/Поиск ЛС по ФИО проживающего")]
        public async Task<ActionResult<Resident>> GetByName(string name)
        {
            name = name.Replace(" ", "");
            Resident resident = await db.Residents.FirstOrDefaultAsync(x => x.Names.Contains(name));
            if (resident == null)
                return NotFound();
            return new ObjectResult(resident);
        }
        //Только ЛС с проживающими
        [HttpGet("/Только ЛС с проживающими")]
        public async Task<ActionResult<IEnumerable<Resident>>> GetWithResidents()
        {
            return await db.Residents.Where(c => c.CountResidents > 0).ToListAsync();
        }

        //Только открытые на указанную дату ЛС 
        [HttpGet("/Только открытые на указанную дату ЛС ")]
        public async Task<ActionResult<IEnumerable<Resident>>> GetInDate(DateTime date)
        {
            return await db.Residents.Where(c => c.DateEnd >= date).Where(c => c.DateBegin <= date).ToListAsync();
        }

        // Метод POST
        [HttpPost("/Создать новый ЛС")]
        public async Task<ActionResult<Resident>> Post(Resident resident)
        {
            if (resident == null)
            {
                return BadRequest();
            }
            
            int countNames = resident.Names.Split(',').Length; //количество проживающих по указанным ФИО

            resident.Names = resident.Names.Replace(" ", ""); //извлечение лишних пробелов
            resident.Age = resident.Age.Replace(" ", ""); 

            var residents = resident.Names.Split(','); //Превращение в массив для облегчения работы
            var residentsAge = resident.Age.Split(',');

            resident.NamePayer = residents[0]; //имя плательщика (указывается в первую очередь в Names)

            //проверка совпадения имен и возрастов проживающих
            if (!isCorrectAges(residentsAge) || !isCorrectNames(residents))
            {
                ModelState.AddModelError("Names, Age", "Некорректные данные о проживающих");
            }

            //Проверка на количество проживающих
            if (countNames != residentsAge.Length)
            {
                ModelState.AddModelError("CountResidents", "Некорректные данные о проживающих");
            }         

            //валидация данных. Для Указания всех ошибок при неправильном вводе, используется if, а не else if
            if (resident.Names == "string" || resident.Names == "")
            {
                ModelState.AddModelError("Name", "Недопустимые имена жителей");
            }
            if (resident.Address == "string" || resident.Address == "")
            {
                ModelState.AddModelError("Address", "Недопустимый адрес проживающих");
            }
            if (resident.Age == "string" || resident.Age == "")
            {
                ModelState.AddModelError("Age", "Недопустимый возраст проживающих");
            }
            if (resident.DateBegin > resident.DateEnd)
            {
                ModelState.AddModelError("DateBegin", "Неккоректная дата");
            }

            /*
             * Проверка на случай, если никто не проживает, но ЛС зарегистрирован на человека
             * Также, ещё проверка на правильное указание количества проживающих
             */
            if ((resident.CountResidents == 0 && countNames - 1 > 0) || (resident.CountResidents > 0 && resident.CountResidents != countNames))
            {
                ModelState.AddModelError("CountResidents", "Недопустимое количество проживающих");
            }          
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            resident.NumberLc = GenerateNumberLc(); //генерация случайного номера. Для тестирования используется случайный номер
            resident.DateBegin = DateTime.MinValue; //Дата открытия
            resident.DateEnd = DateTime.Now; //Дата закрытия
            db.Residents.Add(resident);
            await db.SaveChangesAsync();
            return Ok(resident);
        }

        // Метод PUT
        //Чтобы не было ошибок при редактировании данных, повторяю код :(
        [HttpPut("/Обновить данные")]
        public async Task<ActionResult<Resident>> Put(Resident resident)
        {
            if (resident == null)
            {
                return BadRequest();
            }

            int countNames = resident.Names.Split(',').Length;
            resident.Names = resident.Names.Replace(" ", "");
            resident.Age = resident.Age.Replace(" ", "");

            var residents = resident.Names.Split(',');
            var residentsAge = resident.Age.Split(',');

            resident.NamePayer = residents[0];

            if (!isCorrectAges(residentsAge) || !isCorrectNames(residents))
            {
                ModelState.AddModelError("Names, Age", "Некорректные данные о проживающих");
            }
            if (countNames != residentsAge.Length)
            {
                ModelState.AddModelError("CountResidents", "Некорректные данные о проживающих");
            }
            if (resident.Names == "string" || resident.Names == "")
            {
                ModelState.AddModelError("Name", "Недопустимые имена жителей");
            }
            if (resident.Address == "string" || resident.Address == "")
            {
                ModelState.AddModelError("Address", "Недопустимый адрес проживающих");
            }

            if (resident.Age == "string" || resident.Age == "")
            {
                ModelState.AddModelError("Age", "Недопустимый возраст проживающих");
            }

            if ((resident.CountResidents == 0 && countNames - 1 > 0) || (resident.CountResidents > 0 && resident.CountResidents != countNames))
            {
                ModelState.AddModelError("CountResidents", "Недопустимое количество проживающих");
            }
            if (resident.DateBegin > resident.DateEnd)
            {
                ModelState.AddModelError("DateBegin", "Неккоректная дата");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            resident.NumberLc = GenerateNumberLc();
            db.Update(resident);
            await db.SaveChangesAsync();
            return Ok(resident);
        }

        // Метод DELETE. Удаляет по id
        [HttpDelete("/Удаление по id")]
        public async Task<ActionResult<Resident>> Delete(int id)
        {
            Resident resident = db.Residents.FirstOrDefault(x => x.Id == id);
            if (resident == null)
            {
                return NotFound();
            }
            db.Residents.Remove(resident);
            await db.SaveChangesAsync();
            return Ok(resident);
        }

        //удаление по номеру ЛС
        [HttpDelete("/Удаление по номеру ЛС")]
        public async Task<ActionResult<Resident>> Delete(string numberLc)
        {
            Resident resident = db.Residents.FirstOrDefault(x => x.NumberLc.Contains(numberLc));
            if (resident == null)
            {
                return NotFound();
            }
            db.Residents.Remove(resident);
            await db.SaveChangesAsync();
            return Ok(resident);
        }

        //Создание случайного номера ЛС
        private string GenerateNumberLc()
        {
            string s = "";
            for (int i = 0; i < 8; i++)
            {
                Random r = new Random();
                s += r.Next(0, 9).ToString();
            }
            var l = db.Residents.Where(u => u.NumberLc.Contains(s));
            if (l.Count() > 0)
            {
                GenerateNumberLc();
            }
            return s;
        }
        //проверка на правильность ввода возраста.
        //P.S. Чтобы избежать корректный ввод номера и имени, использую два различных метода
        private bool isCorrectAges(string[] ages)
        {
            bool digitsOnly = true;
            foreach (var st in ages){
                digitsOnly = st.All(char.IsDigit);
                if (!digitsOnly) return digitsOnly;
            }
            return digitsOnly;
        }

        //проверка на правильность ввода имени
        private bool isCorrectNames(string[] names)
        {
            bool isLetters = true;
            foreach (var st in names)
            {
                isLetters = st.All(Char.IsLetter);
                if (!isLetters) return isLetters;
            }
            return isLetters;
        }
        
    }
}
