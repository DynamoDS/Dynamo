import os
from pyspark.sql import SparkSession
from pyspark.sql import functions as F
from pyspark.sql.window import Window

def run_pipeline():
    spark = SparkSession.builder.appName("LedgerReconciliation").master("local[*]").getOrCreate()

    tx_path = "/app/data/transactions_raw.parquet"
    acc_path = "/app/data/accounts_dim.csv"
    out_path = "/app/output/account_balances/final_balances.parquet"

    df_acc = spark.read.option("header", "true").csv(acc_path).filter(F.col("status") != "CLOSED")
    df_tx = spark.read.parquet(tx_path)

    df_joined = df_tx.join(df_acc, "account_id", "inner") \
        .withColumn("utc_ts", F.to_utc_timestamp(F.col("event_timestamp"), F.col("timezone"))) \
        .filter(F.col("utc_ts") < F.lit("2024-01-01 00:00:00"))

    dedup_window = Window.partitionBy("tx_id", "event_type", "seq_num") \
        .orderBy(F.col("event_timestamp").desc(), F.col("retry_flag").asc(), F.col("amount_cents").desc())

    df_dedup = df_joined.withColumn("rk", F.row_number().over(dedup_window)).filter(F.col("rk") == 1).drop("rk")

    df_agg = df_dedup.groupBy("account_id").agg(
        F.coalesce(F.sum(F.when(F.col("event_type") == "CAPTURE", F.col("amount_cents")).otherwise(0)), F.lit(0)).cast("long").alias("settled_balance_cents"),
        F.coalesce(F.sum(F.when(F.col("event_type") == "AUTH", F.col("amount_cents")).otherwise(0)) - 
                   F.sum(F.when(F.col("event_type") == "CAPTURE", F.col("amount_cents")).otherwise(0)), F.lit(0)).cast("long").alias("pending_balance_cents"),
        F.coalesce(F.max("seq_num"), F.lit(-1)).cast("long").alias("last_processed_seq_num")
    )

    df_final = df_acc.select("account_id").join(df_agg, "account_id", "left") \
        .fillna({"settled_balance_cents": 0, "pending_balance_cents": 0, "last_processed_seq_num": -1}) \
        .orderBy("account_id")

    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    df_final.toPandas().to_parquet(out_path, index=False)
    print(f"Output saved to {out_path}")
    spark.stop()

if __name__ == "__main__":
    run_pipeline()
