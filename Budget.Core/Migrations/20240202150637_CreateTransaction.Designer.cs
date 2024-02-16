﻿// <auto-generated />
using System;
using Budget.Core.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Budget.Core.Migrations
{
    [DbContext(typeof(BudgetContext))]
    [Migration("20240202150637_CreateTransaction")]
    partial class CreateTransaction
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Budget.Core.Models.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasPrecision(12, 2)
                        .HasColumnType("numeric(12,2)")
                        .HasColumnName("amount");

                    b.Property<string>("AuthorizationCode")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("authorization_code");

                    b.Property<decimal>("BalanceAfterTransaction")
                        .HasPrecision(12, 2)
                        .HasColumnType("numeric(12,2)")
                        .HasColumnName("balance_after_transaction");

                    b.Property<DateOnly?>("CashbackForDate")
                        .HasColumnType("date")
                        .HasColumnName("cashback_for_date");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("character varying(5)")
                        .HasColumnName("currency");

                    b.Property<DateOnly>("DateTransaction")
                        .HasColumnType("date")
                        .HasColumnName("date_transaction");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("description");

                    b.Property<int>("FollowNumber")
                        .HasColumnType("integer")
                        .HasColumnName("follow_number");

                    b.Property<string>("Iban")
                        .IsRequired()
                        .HasMaxLength(34)
                        .HasColumnType("character varying(34)")
                        .HasColumnName("iban");

                    b.Property<string>("IbanOtherParty")
                        .HasMaxLength(34)
                        .HasColumnType("character varying(34)")
                        .HasColumnName("iban_other_party");

                    b.Property<string>("NameOtherParty")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name_other_party");

                    b.HasKey("Id")
                        .HasName("pk_transactions");

                    b.HasIndex("FollowNumber", "Iban")
                        .IsUnique()
                        .HasDatabaseName("ix_transactions_follow_number_iban");

                    b.ToTable("transactions");
                });
#pragma warning restore 612, 618
        }
    }
}