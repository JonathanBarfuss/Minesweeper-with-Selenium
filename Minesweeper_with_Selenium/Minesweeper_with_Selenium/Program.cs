using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;

namespace Minesweeper_with_Selenium
{
    class Program
    {
        static void Main(string[] args)
        {
            int rows = 20;
            int cols = 40;
            int mines = 150;
            
            Random rand = new Random();

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            IWebDriver driver = new ChromeDriver(/*chromeOptions*/);
            driver.Manage().Window.Maximize();
            try
            {
                MinesweeperPage page = new MinesweeperPage(driver);

                Display display = page.openDisplays();
                display.changeZoom(200);
                display.close();

                Game gameTab = page.openGame();
                gameTab.changeDifficulty(rows, cols, mines);
                gameTab.newGame();

                Minesweeper game = new Minesweeper(rows, cols, page);

                game.selectSquare(rand.Next(rows), rand.Next(cols));
                while(!page.gameLost() && !page.gameWon())
                {
                    game.analyzeForFlags();
                    game.analyzeClicks();
                    if (page.getNumClicks() == 0)
                        game.analyzeDoubles();
                    if(page.getNumClicks() == 0)
                    {
                        game.selectSquare(rand.Next(rows), rand.Next(cols));
                    }
                    page.resetClicks();
                }
                game.searchMines();
                game.display();

            }
            finally
            {
                System.Threading.Thread.Sleep(2000);
                driver.Quit();
            }

            
        }
    }
}
