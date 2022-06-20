using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;

namespace Minesweeper_with_Selenium
{
    /// <summary>
    /// Class to manipulate the minesweeper page
    /// </summary>
    class MinesweeperPage
    {
        protected IWebDriver driver;
        private By gameBy = By.Id("game");
        private By faceBy = By.Id("face");
        private By gameTabBy = By.Id("options-link");
        private By displayTabBy = By.Id("display-link");
        private By controlsTabBy = By.Id("controls-link");
        private By importTabBy = By.Id("import-link");
        private By exportTabBy = By.Id("export-link");
        private int numClicks;
        
        public MinesweeperPage(IWebDriver driver)
        {
            this.driver = driver;
            numClicks = 0;

            driver.Navigate().GoToUrl("https://minesweeperonline.com/");
            var verify = new WebDriverWait(driver, TimeSpan.FromSeconds(3)).Until(e => e.FindElement(gameBy));
        }

        //Manipulation methods------------------------------
        /// <summary>
        /// </summary>
        /// <param name="row">The row of the square</param>
        /// <param name="col">The column of the column</param>
        /// Method to left click a square on minesweeper page
        public void selectSquare(int row, int col)
        {
            try
            {
                if (gameLost())
                    return;
                driver.FindElement(By.Id(row + "_" + col)).Click();
                numClicks++;
            }
            catch
            {
                Console.WriteLine("Could not select the square on row " + row + " and column " + col);
            }
        }

        /// <summary>
        /// Method to learn the value of a square on the Minesweeper page
        /// </summary>
        /// <param name="row">The row of the square</param>
        /// <param name="col">The column of the square</param>
        /// <returns>The square's value</returns>
        public int readSquare(int row, int col)
        {
            //Negative value means blank
            
            try
            {
                if (gameLost())
                    return -1;
                IWebElement square = driver.FindElement(By.Id(row + "_" + col));
                String state = square.GetAttribute("class");
                if(state == "square blank")
                {
                    return -1;
                }

                return state[state.Length - 1] - '0';
            }
            catch
            {
                Console.WriteLine("Could not read the value at row " + row + " and col " + col);
                return -1;
            }
        }

        public bool isBomb(int row, int col)
        {
            try
            {
                IWebElement square = driver.FindElement(By.Id(row + "_" + col));
                String state = square.GetAttribute("class");
                if (state == "square bombrevealed")
                    return true;
                else
                    return false;
            }
            catch
            {
                Console.WriteLine("Could not check if the square at row " + row + " and col " + col + " was a mine");
                return false;
            }
        }

        public bool isDeath(int row, int col)
        {
            try
            {
                IWebElement square = driver.FindElement(By.Id(row + "_" + col));
                String state = square.GetAttribute("class");
                if (state == "square bombdeath")
                    return true;
                else
                    return false;
            }
            catch
            {
                Console.WriteLine("Could not check if the square at row " + row + " and col " + col + " was the clicked mine");
                return false;
            }
        }

        /// <summary>
        /// Method to right click a square on the Minesweeper page
        /// </summary>
        /// <param name="row">The row of the square</param>
        /// <param name="col">The column of the square</param>
        public void flagSquare(int row, int col)
        {
            /*try
            {
                if (gameWon() || gameLost())
                    return;
                IWebElement square = driver.FindElement(By.Id(row + "_" + col));
                Actions action = new Actions(driver);
                action.ContextClick(square).Perform();
                numClicks++;
            }
            catch
            {
                Console.WriteLine("Could not flag the square at row " + row + " and col " + col);
            }*/
        }

        //Support methods-------------------------------------
        /// <summary>
        /// Method that tells whether the game is in a lost state or not
        /// </summary>
        /// <returns>True if the game is lost. False otherwise</returns>
        public bool gameLost()
        {
            try
            {
                if (driver.FindElement(faceBy).GetAttribute("class") == "facedead")
                {
                    return true;
                }
                return false;
            }
            catch
            {
                Console.WriteLine("Faled to read victory/loss status");
                return false;
            }
        }

        /// <summary>
        /// Method that tells whether the game is in a victory state or not
        /// </summary>
        /// <returns>True if the game is won. False otherwise</returns>
        public bool gameWon()
        {
            try
            {
                if (driver.FindElement(faceBy).GetAttribute("class") == "facewin")
                {
                    return true;
                }
                return false;
            }
            catch
            {
                Console.WriteLine("Failed to read victory/loss status");
                return false;
            }
        }

        public bool gameRecord(String name)
        {
            try
            {
                driver.SwitchTo().Alert().Dismiss();
                Console.WriteLine("Success");
                /*driver.SwitchTo().Alert().SendKeys(name);
                driver.SwitchTo().Alert().Accept();*/
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns how many left & right clicks have been made since last reset
        /// </summary>
        /// <returns>Number of clicks</returns>
        public int getNumClicks()
        {
            return numClicks;
        }

        /// <summary>
        /// Resets the number of left & right clicks to 0
        /// </summary>
        public void resetClicks()
        {
            numClicks = 0;
        }

        //Tab methods-----------------------------
        public Display openDisplays()
        {
            driver.FindElement(displayTabBy).Click();
            return new Display(driver);
        }

        public Game openGame()
        {
            driver.FindElement(gameTabBy).Click();
            return new Game(driver);
        }
    }

    class Display
    {
        protected IWebDriver driver;
        private int zoom;
        private bool position;      //true is center, false is left
        private bool nightMode;

        private bool closed;

        public Display(IWebDriver driver) {
            this.driver = driver;
            zoom = 100;
            position = true;
            nightMode = false;
            closed = false;
        }

        public void changeZoom(int zoom)
        {
            if (closed)
                return;
            this.zoom = zoom;
            driver.FindElement(By.Id("zoom" + zoom)).Click();
        }

        public void close()
        {
            driver.FindElement(By.Id("display-close")).Click();
            closed = true;
        }
    }

    class Game
    {
        protected IWebDriver driver;
        public enum difficulty { BEGINNER, INTERMEDIATE, EXPERT, CUSTOM };
        private difficulty diff;

        public Game(IWebDriver driver)
        {
            this.driver = driver;
            diff = difficulty.EXPERT;
        }

        /// <summary>
        /// Adjusts the difficulty setting of the game. Either enter difficulty.BEGINNER, difficulty.INTER..., etc.
        /// Use the "newGame()" function to apply new difficulty
        /// </summary>
        /// <param name="x">Difficulty (beginner, intermediate, or expert)</param>
        public void changeDifficulty(difficulty x)
        {
            try
            {
                switch (x)
                {
                    case difficulty.BEGINNER:
                        driver.FindElement(By.Id("beginner")).Click();
                        break;
                    case difficulty.INTERMEDIATE:
                        driver.FindElement(By.Id("intermediate")).Click();
                        break;
                    default:
                        driver.FindElement(By.Id("expert")).Click();
                        break;
                }
            }
            catch
            {
                Console.WriteLine("Could not adjust the difficulty");
            }
        }

        /// <summary>
        /// Change the difficulty to a custom difficulty
        /// </summary>
        /// <param name="height">How many rows in the game</param>
        /// <param name="width">How many columns in the game</param>
        /// <param name="mines">How many mines in the game</param>
        public void changeDifficulty(int height, int width, int mines)
        {
            try
            {
                driver.FindElement(By.Id("custom")).Click();
                driver.FindElement(By.Id("custom_height")).SendKeys(Keys.Backspace + Keys.Backspace + height);
                driver.FindElement(By.Id("custom_width")).SendKeys(Keys.Backspace + Keys.Backspace + width);
                driver.FindElement(By.Id("custom_mines")).SendKeys(Keys.Backspace + Keys.Backspace + Keys.Backspace + mines);
            }
            catch
            {
                Console.WriteLine("Could not adjust the difficulty");
            }
        }

        public void newGame()
        {
            try
            {
                driver.FindElement(By.XPath("//*[@id='options']/tbody/tr[7]/td[1]/input")).Click();
            }
            catch
            {
                Console.WriteLine("Could not start a new game");
            }
        }

        public void close()
        {
            try
            {
                driver.FindElement(By.Id("options-close")).Click();
            }
            catch
            {
                Console.WriteLine("Could not close the Game menu");
            }
        }
    }
}
