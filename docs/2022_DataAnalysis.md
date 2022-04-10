# Treatement and analysis of 2022 r/place data

The raw dataset can be found here: https://www.reddit.com/r/place/comments/txvk2d/rplace_datasets_april_fools_2022/

The full csv file is here: https://placedata.reddit.com/data/canvas-history/2022_place_canvas_history.csv.gzip (12Go gzipped, 21,2GO unzipped, 160 353 104 lines without the header).

There was 160 353 085 tiles placed and 19 moderators interventions.

## Color map

index | hex color | rgb           |human color
------|-----------|---------------|------------
0     |  6D001A   | (109,0,26)    | darkest red
1     |  BE0039   | (190,0,57)    | dark red
2     |  FF4500   | (255,69,0)    | red
3     |  FFA800   | (255,168,0)   | orange
4     |  FFD635   | (255,214,53)  | yellow
5     |  FFF8B8   | (255,248,184) | pale yellow
6     |  00A368   | (0,163,104)   | dark green
7     |  00CC78   | (0,204,120)   | green
8     |  7EED56   | (126,237,86)  | light green
9     |  00756F   | (0,117,111)   | dark teal
10    |  009EAA   | (0,158,170)   | teal
11    |  00CCC0   | (0,204,192)   | light teal
12    |  2450A4   | (36,80,164)   | dark blue
13    |  3690EA   | (54,144,234)  | blue
14    |  51E9F4   | (81,233,244)  | light blue
15    |  493AC1   | (73,58,193)   | indigo
16    |  6A5CFF   | (106,92,255)  | periwinkle
17    |  94B3FF   | (148,179,255) | lavender
18    |  811E9F   | (129,30,159)  | dark purple
19    |  B44AC0   | (180,74,192)  | purple
20    |  E4ABFF   | (228,171,255) | pale purple
21    |  DE107F   | (222,16,127)  | magenta
22    |  FF3881   | (255,56,129)  | pink
23    |  FF99AA   | (255,153,170) | light pink
24    |  6D482F   | (109,72,47)   | dark brown
25    |  9C6926   | (156,105,38)  | brown
26    |  FFB470   | (255,180,112) | beige
27    |  000000   | (0,0,0)       | black
28    |  515252   | (81,82,82)    | dark gray
29    |  898D90   | (137,141,144) | gray
30    |  D4D7D9   | (212,215,217) | light gray
31    |  FFFFFF   | (255,255,255) | white

## First reformat

Using a simple python script, remove the useless ` UTC` at the end of timestamp, reformat the coordinates and remove moderator interventions. (later we will take it in account)

The script find 20 lines containing 4 coordinates, that mean there were 20 moderators interventions. For the following, they are ignored.

## SQL lite import

Import the CSV file as a new table. The table should have this schema:

```sql
CREATE TABLE "place_tiles" (
    "timestamp" TEXT,
    "user_hash" TEXT,
	"x_coordinate"	INTEGER,
	"y_coordinate"	INTEGER,
	"color"	INTEGER
)
```

Create indexes

```sql
--Create index on timestamp
CREATE INDEX "timestamp_index" ON "place_tiles" (
	"timestamp"
);
--552864 ms

--Create index on user_hash
CREATE INDEX "user_hash_index" ON "place_tiles" (
	"user_hash"
);
--1367560 ms

--Create index on x_coordinate
CREATE INDEX "x_coordinate_index" ON "place_tiles" (
	"x_coordinate"
);
--544516 ms

--Create index on y_coordinate
CREATE INDEX "y_coordinate_index" ON "place_tiles" (
	"y_coordinate"
);
--553405 ms

--Create index on color
CREATE INDEX "color_index" ON "place_tiles" (
	"color"
);
--493056 ms
```

## Generating CSV for PlaceVRsualizer

```sql
CREATE VIEW place_tiles_sorted_no_user
AS
SELECT timestamp as ts, x_coordinate, y_coordinate, color FROM place_tiles ORDER BY ts, x_coordinate, y_coordinate ASC;
```

Export this view as CSV file.

## Basic analysis

Total number of tiles:
```sql
SELECT COUNT(*) FROM place_tiles;
```
| count     |
|-----------|
| 160353085 |

Color usage:
```sql
SELECT color, COUNT(*) as count FROM place_tiles GROUP BY color ORDER BY count DESC;
--79738ms
```
code | count
-----|------
27	 | 33707367
31	 | 32251013
2	 | 14411389
12	 | 9989853
4	 | 8519392
1	 | 5911641
14	 | 5700301
18	 | 5245484
3	 | 5059970
23	 | 4917801
13	 | 4058046
6	 | 3892844
29	 | 3459386
8	 | 3417232
30	 | 3324082
25	 | 2473639
26	 | 2104844
22	 | 1458772
19	 | 1287671
24	 | 1261416
7	 | 1200066
15	 | 1139350
5	 | 954604
28	 | 868769
0	 | 621194
21	 | 589211
9	 | 572572
16	 | 499233
17	 | 454140
10	 | 436068
20	 | 350873
11	 | 214862

User places tiles:
```sql
SELECT user_hash, COUNT(*) as count FROM place_tiles GROUP BY user_hash ORDER BY count DESC LIMIT 100;
```

user_hash                                                                                | count
-----------------------------------------------------------------------------------------|------
kgZoJz//JpfXgowLxOhcQlFYOCm8m6upa6Rpltcc63K6Cz0vEWJF/RYmlsaXsIQEbXrwz+Il3BkD8XZVx7YMLQ== | 795
JMlte6XKe+nnFvxcjT0hHDYYNgiDXZVOkhr6KT60EtJAGaezxc4e/eah6JzTReWNdTH4fLueQ20A4drmfqbqsw== | 781
LNbGhj45pAeCvBYQF1dPvwx1zVfVTy8AdRxTSHi0pR9YeabE3sAd3Rz1MbLFT5k14j0+grrVgqYO1/6BA/jBfQ== | 777
8USqGo14WuZQLG7PSAwqfFwICkU0G4VyHZTuV8D1QSbQHE5GFdC2mIK/pMEC/qF1FQH912SDim3ptEFkYPrYMQ== | 767
K54RRTUCFuOU55RzSTkjo/ftbJqVi9miyt52YV6NlENRfUyJTPJKBC47N/s2eh4iNdAKMKxa3gvL2XFqCc9AqQ== | 767
DspItMbX4x7ZD/Ozqke3BL3IQs40A3suoSQ8mb5V7Nu8Z1nYWWzGaFSj7UtRC0W75P7JfJ3W+4ne36EiBuo2YQ== | 766
6QK00igvPdzUYm9SEriCDTbwTmM3RcY17Ynr9FRA6PfLKMUNur4cedRmY9wX4vL6bBoV/JW/Gn6TRRZAJimeLw== | 765
VenbgVzRUq0U7MX3agINB3qBtjbEkZh8HmPSQHnkhkTwy/w5C6jodImdPn6bM8izTHI66HK17D4Bom33ZrwuGQ== | 758
jjtKU98x1Bc/qYVoVBq/gQd6yaFiaTrb2vT6fp+Ias+PeaHE9vWC4g7p2KJKLBdjKvo+699EgRouCbeFjWsjKA== | 730
VHg2OiSkbBCDTTl1W41fdvDSJk/QtKsdsufSZomBPV3cr2K8+0RW4ILyT1Bmot0bU3bOJyHRPW/w60Y5so4F1g== | 713
TMlARGMJdmz7l+tFyl35z+/JmxfKQ4v+J5R49Hsin840+Wa+bzWNhQuj0/1L/FU2Hr36mrM9KAjCFtZfSfeTrg== | 707
/bHvPrpxCR0z2XyW50uCG277FzgDmdN88mW5MsAHDkZVR0+d3Dqu5pcYS/q9DWzpQlQ7Ah2TQTavZXaBXZqYWQ== | 693
/WGJoCWq1YMVOceGyK6aJKBuXZemxkEjC5PQRybz44A+lYcxITkMOQLc0nCl5URJi9yaAHhBp1aTt6ed48chiw== | 692
M5ALAb3N0o/fsoBJPcgb4K2bQZvbrKBEUjnGn8ilMF6pz4vQVWHwaTdq2z6tYo1ASQTrZHoJIaeSNywc0Pl1PA== | 678
2Q3uUJ/COj8cEentypGuXyGKXzkvUCOHLVJtHNyWjd4XB+XdpVJ+RnwlaaAPrLBK6t6L32sK63aPWScT3TEq4w== | 673
6/vtU6LdHfsQl/+NxJRyQdsyeXr2dwkHuQunGX+9xRV4I+ca6H7ZjLcchmQYB12Nhl37ZasMqy76gICQO4SYDw== | 670
HUChBTJUWVtsb7nPdwDGS/NBvZqg4HvqNxo3AZbbU+VxEf0jLyGUvLyLbv2rQPcWbFL/mHPqGuDfhgkC1U2qVA== | 670
RBLMaPaOq15DJvgEvXKatS8pUAQvsRTG9YrBdvh7TRaPSXsTgxnEGoTeswN3d7g3tvOv/FaXDoC7MB72erfSvA== | 670
BY9Nk55dKnEVcYGos/8YKyQnr5zU4GXVXLrUaX67orx05IKAEWwes0xzUa4wS2YXUXOyKZ9Fdc4Y+1tkLg3p2Q== | 658
E4eoXRF/Ewkz6QHjkud/j6RYLWinar5BPzdfsOJNakLzmAIqDML2XbhCoOirqLfdDFQG2YFD01ofkuFVVo6WRw== | 656
oIJZRgTHdz4k3wULAhBJfm4SzP73AFaj1q/ZXlBjyMaWV9lPSDCTcAmBwH3ef9gHOyhTw0FpC6epBCkdb2BusQ== | 656
Lxuvbeef02HmX+eVsxhiCKlyngx1GgpZ34d/LhzGWfhJEkh0hz1dIu0uJ7kSbOwhtnJI6fxKeHjBysYz4cXMKw== | 654
c91/+VbtupjN8rzUI/jv2NV1EdWBY9dMvV6n0WnqySAl/uK7+nuuXEuhiZpb1xG9MAJNzvZzoPdFCjINDMCxvA== | 654
+PueYceT9lUiXKAVAJ+eDwsjCegz34be9rYz/2Zm7/rio5wGxg4WO0tGCUC94mPs1uVSfV4HmHXRifRSrdmSpg== | 653
UQjLeYWynn2j8p4S26Q7R1Mt5zfmm3rme4/OIeIVDD54xOZ470SR9T0a99gCX40zh7Yynv+ufzjNLy/K5womfw== | 653
Q2ZJu9YSOw0I5EgH914csuv0a94eQZOZgG7/ll3b1IAVSo9Wwr0oc+5/1sCXfGdZAsAEM8D8u3gxpl5NhXyGGQ== | 652
kzwVTZdo4dhnMq1BDYPmBzThW9tDp3RWsROcu7v13LCZLgbZFxHj8nNGtk9ACcAeZZG9F8TFFI+v1CSTUN1dBw== | 652
wQQeSOxH68BM/lx1ZQDs9C+H5okeMGwOE4DjhakoirTGtlxsDI7zkuZHMWdMri+D98OVwK+LDNtkpc4eicn6Zw== | 652
Hboa1dBGvohKWmS6XWWFQiLhPU6iLlHXCOLL2IDWOqHCLZaHzSYGzFP0Smro5JXBBx+IuA21lhdBFJhS2108MA== | 651
PYoPpVV6ZilhnUipoIPUrIvV6jsB0Bsn8LoYHmhHx20VSCk3e5k5cQMRcxAj4mm1v820NJvjQAf+12JJ9zmD2w== | 646
SyjBDoQsxibLrYD5MpBMkeUYicbnJZzcQoFFF1WOApzJVnnJ6YJaA/gkVGyTrAfvBFL6/X8x3Nn9udDRlnuKXg== | 646
epzSza5+CZKI5jPxQOtFDZlpBzaGAvd+uhFNevCUm350PbActXyR/wB+0txU/HXfAD/eTfd3Igay9p6QcQvVwA== | 645
fj5xJ/zyiBuk5vycbroDO7AmlVJZTw1AJZtM2M+o1nBefLcNyKJNpuv3mMYfhuBpRtk5Haejd9W8q5NvnEJoZg== | 643
M48x26ArtGQtiP6OYZwh2fiUIhuTDM0V+MNGIrK5lwfnw+0zfj1ewzqAYMChwnSGTC2/6JeBvmORxs/Q3Mqemg== | 642
v98DGryv83inBTCrvZqIVYAk/Ya1b6eZPRNP5joWl5gvKDuBsWnsYK7F6fF/jDlK0bJJN6WSL7R+yQFvbZUZVQ== | 641
F2rmon78HghmKTa/o+12ZsqQxWateaygrmmxgrPQU8k2L3sulRDg1Qhj1rKG0W1MpKrkgLToZeO+jHNTY16LMw== | 640
XqPsRPpIhRTLJ0P2dXgHGAUFt49MlhRAASwR5J9YJmCMB/As6NZ4asZndMVQLAgPzM5NV5w0R2kwKejDBybvPQ== | 640
t7B3xoeQKn6txmmSew0uSnjmkibacKGQPl73//roZARduD1E42Ft+kd8dPU9dGEEQLUL9pmYlEiaePQ0jOOWfQ== | 639
TK4abXzi0J+Wb//EiEYVZgLpcegyQ4AkWsna1uKtZ5S+CflzxIEuLM9QGLdloLZWFHHwmzXu8SKOXNvxtNmZvA== | 638
f290yHdM3WAXSaQmRwIBWrxMlLtdkOkZ9Dv0ZXTF7LC6AQDedt4ZgmtS6E2hp4uNXj93AFwady3xzv8xWrBrOw== | 637
WxRshRaXtIweLY+IivVA8DUTVCftKAZXbBbr0jYin2pcpEHNhr+oi51PLN8iXVOr1WL7Q4+phw345ApsZZzVSw== | 634
rNAS+zplIHGaxFdwBg3HYExAwJOL+3oYn6+aOsoHlEgbUqxPdp3ybAvOOfF5AGmzZYzLvztbVkw4geLOn3Cepg== | 634
ApkMk95A9YFOJCmIOtUytfzon8Bb9JlqmElicI0rHytZ4nOGZtLKnN3AyhI9fl2zMwQu1q50AC1B1S/p0K6IAg== | 633
tzZ60n2btnTeVhDYbz0K1aY02e05kL3znMwx1sR8EKh8LGNN7cQzOG3XeEcaTb5eNNN3GxVBSiQA1O6ie89SRg== | 633
6k6H1h0ZXoV/H6hR9t43JxUDoBDXQL4nfhd02iau1Rq7yteOwlRa0kWwLTI7VFKiMP2eeZ5xs8SByL6HiBUQmQ== | 632
HCpQrPCWRghDrBiaCDuezSHOd7W1dr5hKlpPznWfYw5pHd6moo7kVw24tXU8F9ULz8qvB3aRAY9TKSb+YwZJWg== | 631
v6kLqKdX7PVgX2iA9HTEcvh+z0ApagXh2AhX6tNPUqak3EUkSN19F1kRxMeZ85C2r29Ui1r4twwJlZlITL+vow== | 630
x6gEcbhknoLgBtsuNTsiI6XsyOuTZ/7tiScghCNNmTB6YmaINRj9bxQVvFzyRxD7VibwDoVkKV5PjQJAetsXdA== | 630
1iVzIficenzO3EVUIr9h/B0TSHGGEem+ynjsi90O/OE96limvrO8LzM0SpozW2XC2E37Pt73c9dw0PBNh2b30A== | 629
SA1yMw6UeWP7xQivK86InILkfuH9FcP1DBGlA6JnAJZoEwOAetc8fI+hUYhOwxjCst8m0D4DTkb8yWnnGw1WAg== | 629
ERzHXb3SKcJLkZWBU61Ydax//XgOlDOKAOkUBgBz3ilFF6AIFewMYA4yr2QluIfryqlyQKUvBMcS7yqLD18+DQ== | 628
RRvmknLVCnotwCjPjRhKoIoAXN65sk2wcfZAy9APcH447jg8Ku7eAOd/cXVy79ko0HoONcHmewg3aBzwXQ8E0A== | 628
er8avl6tF83VIEDJPyxcSJXJsj1oR40sPzAa15UAz//degmENaEDU1jxb2jBslYJV25KmdCiEm/fKxN71TVEpg== | 628
hbxBm3DCxg/eA0n0EoUM0EPSluNZOFLWF3H7MQU6V7t//PMlGXcpvDPxbkyn/CP31LoTIMsuquCwS8XadRE6mg== | 628
8TrccNAFBoBGIrq3c8ijYS/AySlJbINMhx2vXN5ONHEZZ0jxkCpBC6gZFzqby1FcxOEzW00HMHIznlyzYMV4Dw== | 626
lMIjEKrcC5QzymJw3sRmmSOZLikJN9FtSt7hYo/Q+MhOnpm5+wXwwPq5HBkTZAlM1vKJKrVIKKic8Lw9QSg2qg== | 625
FDXypjCzxSkdrDLTmDg35BiVn79blsst7NQ4muVsShsj+mG3bfV6Q2a/xDrgaaMLO8ZOi/NK9YqQ1WbsVk+veQ== | 624
teu7dgtpQuVVvpS1QS/z1i2RD71V9AWAZlBW0MuV+NsiE7jNkhdee3bibdjqRJ8W3nrNt9gLFGY/8L9a4qP6Sw== | 624
4dmGmwlTMWmkgbuu5IwqDYGrQxC4GJhRHFOtuSS6b3Xii8/5c3nK+UJ3pM8dZ9bV2MTZBqiv94pwK1tXsiXiAA== | 623
BwXw3earmVr29vTg1rh8JKm9y94IqnnbLDUtRxkUM8ssVKmgVxU0FSMdUvmSZ/9srfoKTVDXmEIQBlTBtoXINg== | 623
lvMeQP5vEViZ4LCYddCED+KzOQ11ujydr72kg+Jvb+ORD3K+8N/epKMUv4bkVc01LFDF0vTSDdvmVy2hg9mRew== | 623
OKZxIz9gFCV3aAwAaTHNBOT0IZhpyLgsTedy16LVlF9Jn8LXTgLjYHaxB7qo4+ecqSEFO8VlJ08p88jk1wNBWA== | 621
7R1V4L/ZKG1q7VWzLvBytspWIb09LPzQESPnUEeSPOUFPhWpphahuPX9Xh/aSMJvkqC3QPKc7YENxVfmF0XuyQ== | 608
d0ChzwzRmsOR5OEzdig0OzdKLzie6EcwGmKtWNZuWrg7LgPIKI/KCror0frQIqTrwa0MAsz8jfhVaUv0CqCsqQ== | 608
RqQU9rcQS3iq4zRoSaYmOgp7fgFdpKQQerrW/XQvGn/yu20STojeNfkVC4W0qnNZW8NSyHvNZkgI4u9M9MmlvA== | 607
Cg1JJXtW8M2bLe6jHfZahJdS5PpAyyNnxR8mzcZeQxjDhJZF0+mbDYdNmshfmtShZZDh2MMD6Dvx+2Fm8A0roQ== | 606
Tr8uf6+n5LgfAekTr6YkrFijULe3Bt4WfSCw7c2sSeAlufGB3CTlsoo7CV1WL+2Hx7Qd9u2AGA1mpmEtbqvt2Q== | 606
cTMdztPpze1OiEgiB+sbmTkNuPrNdmgHtAgcHqNknnKNji2nlHEMFij469iKs39j4A592t6OrZI4oo8SkY8VWg== | 606
O+96DnflxzeLdBrEnV7qqGeTBjuSL1usN9AEWbj97D00zBhXN0cqB0pNcJ0nFIIAJYhTqF5fmbSwRM9yapavJw== | 605
qYfV/aGzmOqvQERzZkT9QCyCMgrYLHslqzjUVilr4aIjYFuEk5lGrBVb892NbtFQ9Qm2z+B6K3pWqFK9S7Hsow== | 605
Bc6e9PjrRmWlinGWuv2UUNuOBLJ4YlzckFuf4CcbxadzoJ+l18zcjydAnnMIFi5/rZEvH9x8UoshapFjuoEiuQ== | 603
PztY4UG+t6jpymfO0rCMafQWRpOpOgn279Ra0w9FcxzRghxorBAP+ZkydPSm4Qk8LSa8YMoGSSm79MsIAyAQVg== | 603
Ftx90RmcNPUW6Wyu7jMadYIUFlTr3lGYwsyAp/o/hUsciAPKRIjEYiOwS3sFeeraSA+gYK+ksGNgJYLd12JGBg== | 601
z5YP0ZUKpK7HHC4KCZjfpMGYHOG1TZgOLxnuy4nlY9YAt1OqzXla1yg8REjmaLnqUX3ADgh/PD7iFWCpk+dISA== | 600
7ynPdMTj8EX6OiFJr1nWlZF/c7A9W5TGcKZTiBdrwZ4f0ngmSSicuDL+MKyOotFgTXFMgBeWkxVuYDgpYVC6bg== | 599
AzGOHw+g306jdqJJjkS0E6IQb1WmpTpJSs3OMkD7Byx17kK5kH0C+KyCDr8oJ/cEfeOG9r/hZsSpRld+0YY+4Q== | 599
XOcHDPM62MVKKoV6t+yUNnGkwMTBElwsXJU4G6q95sLIauO9OVnL4dNmXWf0pbTQLqGi6sC4Tpcx3GALxGtjjA== | 599
uTJyoBftQ3ETLsUIpcfTOvdv6H/vGNvxpGFUpn/nxoANEG6ve0DpwT5C+4Z6shrL5qxtGiDLxVbKDTljRSuCjQ== | 599
yEbatN6dUdsOV3FgYwa24aTo/XaP2HPsfE+YyOIOqg0zJTXzqGbHY8BjTpFcaWRHA6CMgrb53+seo+rNSh7/6A== | 599
gjP4cUYyvu4KOYxlzi06KRvni97V8TzGYQsRaCigks6kCCMT06HF7PQDP7vhuSzDWdtNuqJ1VA1cIEomMmbo3A== | 598
n0Fyh9S6VYQTwktSvmcQLtDl4CyRhtR5fmU0KquP+6/2nR0pQLAXKj769dA67Gov/ezAHuMVoLKQQuKLZq0zWw== | 597
bPH0X/4y7CEcnWv7BV0LQt2NTEkDAcf8vbuh3Icic5kISpoUNIQ105NC7CiJTBH5c3jwOrvKf13ZdOco9zMoEg== | 596
gYxfrFsr7v9llKvQAc79sNIBw/ABo7TdP7lU9ysTTCxks0VwtFtlq8CIevxU/zyknUwdWgZ+n6bMzK+8Dv1IPw== | 596
muZOJo+kdKlxearnODGhDTAyIJo/jvMt+TgK7pMzeTjsiemyuHvcrSCEzSzRMD4gzs5tImDhwSbFTxZRpaTIaQ== | 596
qSpIxvB1yVb/Jotju0RlS8y9SNVTzRbTxpuYTi0uNgM39JACTSsVWShAwMloKI37RJ6S3u3sXJNukqZNc42k6A== | 596
+1JgW4Wly+kXochax8/a2gFKK6qrgGlphCGl7KcTflmEeTaNcPabeDZVu4g4+lx1PBp+kIt8rB18kpnwaCtNIw== | 594
MxfIYy1aFQRnWMUTrRnAk4thbJqp7NxLfYV91Rp9aX0YhW+J8eU2rkGFt6E4l1LV1o68+aCvOJaTvjTRmerfTg== | 594
SyEU6B8t0GIXQ+6ei87BzJRw8UDM+ofb6dm4V1klazj8/z/a2DGEbDCImuPnvujPteJx5/kaGTUSgRHE6dlWRQ== | 594
UIdZGrrLI2RUbopiuf0tuJWGm4lY9TeosZWXLDUtPoqnk6DLudHI+UJJnEz5KOfh+dVH1A0RGJe03ZTW59/t5Q== | 594
6c1/g42qs9GqrLbuq8dJib76A1o7qIf6GG0/QjQffU1iDNBaSwVjy82y3LRrDDCjChlIzlFs3yLpOwoKMT+aqg== | 593
bBl3nungYJeFqcuWP4QX2Cv56o4bpu4FEyYHgUwEEFWGTFKZo1DIENizBw75JWNx+0o/LpXdnzyJxh/KPFFENg== | 593
fKNBueWS8c3w3vtqClU1+1QRi4qjHXOmX5aWta7QvKQj5aB6evQwiq2s8Lg6GV9rZcKjENT7+mNQE5AJXJGSBQ== | 591
ADF0trF6xcuujAEzeNERFOcwXXdOFnQKTrKqXMSCuhkWnsoMkzYhXdsoI8+j/ckTxju+YJMPND/X6Jlw7KSiVA== | 590
Gd1mkzUnhK9z4QP6qW+MJJ2iYNFKcgi23nN/MZiEb2N5DVHAzL4BVmrJgKhmXpYzlzMLfaf8RKLTMOpIY1x/ow== | 590
Jo9io9oL9Ep2Sj0qsDgONr7ny/9txaYmkQQHIRKaBhbC3fTOGuJUlDupwhDPDTKAZ58w68HUCL8jM+p2SgfEQg== | 590
KbVHm1Hp1IWNKe6IBkPk7HZgX8WFS+JcDt06NZWkb8IY0UCyZTTDBQ39KHl0lOEtLkKNdSnFPo2oGt07iAUyng== | 590
QoXIu3nGGuETMkSjackfmJyodmhinTwQlp9yXx5YXNgvDXLticl6d0z407lvc0VIVv5dypFNsLpMKX4jvPL4Dw== | 590
xE/9iYj+5Ibs1iJBfZTPkl9sb+uanObfFRQ2eobQzRNJj7WfYjEJQlCy1CXsI5dLEtqxy7o+jYwjiHFna/VfyA== | 590
8X5MeaHWObB+v97F5ls365qJbwvVpg/alejZ2Z2yNmAdBcxjSQeAsme4lD7qDEhyKAHnyZZ+yAcszv7Nvq5SOQ== | 589
sHszYnyAVihPXwuTVeIq9sgI1Mf8/78T+A6UFAiZZtc2+j/oP7FSKjKdWhK4dT0qzxqP0CZYYzF5MrUJNhgYDw== | 589
