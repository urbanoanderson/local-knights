// go mod tidy
// go build -o chesspairing.dll -buildmode=c-shared chesspairing_wrapper.go
package main

import "C"

import (
    "context"
    "encoding/json"
    "github.com/gnutterts/chesspairing"
    "github.com/gnutterts/chesspairing/pairing/dutch"
    "github.com/gnutterts/chesspairing/pairing/roundrobin"
)

//export GenerateSwissPairings
func GenerateSwissPairings(input *C.char) *C.char {
    return generatePairings(input, dutch.New(dutch.Options{}))
}

//export GenerateRoundRobinPairings
func GenerateRoundRobinPairings(input *C.char) *C.char {
    return generatePairings(input, roundrobin.New(roundrobin.Options{}))
}

//export GenerateDoubleRoundRobinPairings
func GenerateDoubleRoundRobinPairings(input *C.char) *C.char {
    cycles := 2
    return generatePairings(input, roundrobin.New(roundrobin.Options{Cycles: &cycles}))
}

// shared implementation: receives the pairer directly
func generatePairings(input *C.char, pairer chesspairing.Pairer) *C.char {
    jsonStr := C.GoString(input)

    //logMessage := ""

    // Unmarshal into a generic map
    var data map[string]interface{}
    if err := json.Unmarshal([]byte(jsonStr), &data); err != nil {
        return C.CString("Error parsing input JSON: " + err.Error())
    }

    // Extract players
    var players []chesspairing.PlayerEntry
    if rawPlayers, ok := data["players"].([]interface{}); ok {
        for _, rp := range rawPlayers {
            pmap := rp.(map[string]interface{})
            players = append(players, chesspairing.PlayerEntry{
                ID:          pmap["id"].(string),
                DisplayName: pmap["name"].(string),
                Rating:      int(pmap["rating"].(float64)),
            })
        }
    }

    // Extract rounds
    var rounds []chesspairing.RoundData
    if rawRounds, ok := data["rounds"].([]interface{}); ok {
        for _, rr := range rawRounds {
            rmap := rr.(map[string]interface{})
            games := []chesspairing.GameData{}
            if rawGames, ok := rmap["games"].([]interface{}); ok {
                for _, rg := range rawGames {
                    gmap := rg.(map[string]interface{})

                    games = append(games, chesspairing.GameData{
                        WhiteID: gmap["whiteID"].(string),
                        BlackID: gmap["blackID"].(string),
                        Result:  chesspairing.GameResult(gmap["result"].(string)),
                    })
                }
            }
            byes := []chesspairing.ByeEntry{}
            if rawByes, ok := rmap["byes"].([]interface{}); ok {
                for _, rb := range rawByes {
                    bmap := rb.(map[string]interface{})

                    byes = append(byes, chesspairing.ByeEntry{
                        PlayerID: bmap["playerID"].(string),
                        Type: chesspairing.ByePAB,
                    })
                }
            }
            rounds = append(rounds, chesspairing.RoundData{
                Number: int(rmap["number"].(float64)),
                Games:  games,
                Byes: byes,
            })
        }
    }

    currentRound := int(data["currentRound"].(float64))

    state := &chesspairing.TournamentState{
        Players:      players,
        Rounds:       rounds,
        CurrentRound: currentRound,
        PairingConfig: chesspairing.PairingConfig{
            System: chesspairing.PairingDutch,
        },
    }

    result, err := pairer.Pair(context.Background(), state)
    if err != nil {
        return C.CString("Pairing error: " + err.Error())
    }

    result.Notes = append(result.Notes, "Generated via chesspairing using LocalKnights wrapper")
    //result.Notes = append(result.Notes, logMessage)

    out, _ := json.Marshal(result)
    return C.CString(string(out))
}

func main() {}