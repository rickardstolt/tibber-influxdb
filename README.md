# tibber-influxdb
Fetches consumption, price and cost from Tibber and writes it to InfluxDB.

## How to obtain the Tibber token
- Go to https://developer.tibber.com/ and Sign in.
- Genrerate a token.

## How to run
    docker run -d \
     -e INFLUXDB_HOST="influxdb host" \
     -e INFLUXDB_TOKEN="influxdb token" \
     -e INFLUXDB_ORG="influxdb org"
     -e TIBBER_TOKEN="tibber token" \
     --name "tibber-influxdb" \
    rickardstolt/tibber-influxdb:latest

## All options (with defaults)

### InfluxDB
     -e INFLUXDB_HOST="127.0.0.1" \
     -e INFLUXDB_PORT="8086" \
     -e INFLUXDB_TOKEN="" \
     -e INFLUXDB_ORG="" \
     -e INFLUXDB_BUCKET="tibber" \

### Tibber

     -e TIBBER_TOKEN="" \
     -e TIBBER_HOME="" \

If $TIBBER_HOME is not set, the first home returned by Tibber is used.

### Hours to fetch
If you want to fetch more or less data than the default of one week back.
 
     -e MAXENTRIES="168" \

### Frequency
If you want to fetch data more or less often than the default of 1 hour.

     -e PERIOD="3600" \