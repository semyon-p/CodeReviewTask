
using System;
using System.Text.Json;
using System.Threading;

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


public class Game
{
	private static object _sync = new object ();
	
	private int gameId;
	private ESportType SportType;
	private DateTime _startTime;
	private Referee HeadReferee;
	private Team[] Competitors;
	private string coordinate;

	public Game(ESportType s, DateTime gameStartTime, int gameId, Team home, Team away, Referee headReferee)
	{
		SportType = s;
		_startTime = gameStartTime;
		gameId = gameId;
		Competitors = [home, away];
		HeadReferee = headReferee;
	}
	
	public async void CheckCoordinate()
	{
		Monitor.Enter(_sync);
		try
		{
			string temp = await BallNavigator.GetCoordinate(gameId);
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
	
	public void ChangeReferee(string fn, string ln, DateTime birthDate)
	{
		HeadReferee.FirstName = fn;
		HeadReferee.LastName = ln;
	}
	
	public async void StartGame()
	{
		await GamesDb.Start(gameId, JsonSerializer.Serialize(this));
		await GamesKafkaQueue.SendStart(gameId, JsonSerializer.Serialize(this)));
	}
}