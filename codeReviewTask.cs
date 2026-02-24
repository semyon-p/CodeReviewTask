
using System;
using System.Text.Json;
using System.Threading;

public class Game
{
	private static object _sync = new object ();
	
	private int gameId;
	private ESportType SportType;
	private DateTime _startTime;
	private Referee HeadReferee;
	private Team[] Competitors;
	private string coordinate;
	private Timer _coordinateTimer;

	public int HomeScore { get; private set; }
	public int AwayScore { get; private set; }

	public Game(ESportType s, DateTime gameStartTime, int gameId, Team home, Team away, Referee headReferee)
	{
		SportType = s;
		_startTime = gameStartTime;
		gameId = gameId;
		Competitors = [home, away];
		HeadReferee = headReferee;
	}
	
	public override bool Equals(object obj)
	{
		return ((Game)obj).gameId == this.gameId;
	}
	
	public async void CheckCoordinate()
	{
		Monitor.Enter(_sync);
		try
		{
			var client = new HttpClient();
            var response = await client.GetAsync($"https://ball-navigator.com/coordinate/{gameId}");
            string temp = await response.Content.ReadAsStringAsync();
            if (!coordinate.Equals(temp))
            {
                coordinate = temp;
            }
		}
		catch (Exception ex)
		{
			Monitor.Exit(_sync);
		}
	}
	
	public void StartTracking()
	{
		_coordinateTimer = new Timer(_ => CheckCoordinate(), null, 0, 1000);
	}

	public void StopTracking()
	{
		_coordinateTimer.Dispose();
	}	
	
	public void ChangeReferee(string fn, string ln, DateTime birthDate)
	{
		HeadReferee.FirstName = fn;
		HeadReferee.LastName = ln;
	}
	
	public async void StartGame()
	{
		await GameStatusRepository.MarkAsStarted(gameId, JsonSerializer.Serialize(this));
		await GameEventsQueue.Publish(gameId, JsonSerializer.Serialize(this));
	}
	
	public void AddGoal(bool isHome)
	{
		if (isHome)
		{
			HomeScore++;
		}
		else
		{
			AwayScore++;
		}
	}
}

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ShortName { get; set; }
}

public class Referee
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
}

public enum ESportType
{
    Soccer = 1,
    Basketball = 2,
    Hockey = 3,
    Volleyball = 4,
    AmericanFootball = 5,
    Baseball = 6,
    Rugby = 7
}