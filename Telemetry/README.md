## Instructions

## Collector

To run the collector service, execute the following:

```shell
podman run -p 4317:4317 -p 4318:4318 --rm -v ${PWD}/collector-config.yaml:/etc/otelcol/config.yaml otel/opentelemetry-collector
```

## Backend

See [the OpenTelemetry guide](https://opentelemetry.io/docs/languages/net/exporters/#prometheus) on how to set it all up with Prometheus.

In short, do the following.

Run Prometheus in a docker container with the UI accessible on port `9090`:

```shell
podman run --rm -v ${PWD}/prometheus.yml:/prometheus/prometheus.yml -p 9090:9090 --name prometheus -d prom/prometheus --enable-feature=otlp-write-receiver
```

The rest you can follow in the guide.