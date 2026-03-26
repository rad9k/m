# Genome sequencing


## Sanger Sequencing

Sanger sequencing is a classical, highly accurate method of DNA sequencing based on the selective incorporation of chain-terminating dideoxynucleotides. Often considered the "gold standard" for validating DNA sequences, it is highly effective for targeted, smaller-scale sequencing. Its reliability was recently highlighted during the COVID-19 pandemic, where Sanger sequencing was pivotal in the initial identification of the SARS-CoV-2 virus and remained a crucial tool for confirming specific mutations and variants detected by broader screening methods.

### Overcoming Signal Degradation with Classical Machine Learning

The physical limitations of Sanger sequencing detectors become highly apparent at the tail end of a read. Typically, after the 1,000-base-pair mark, the signal-to-noise ratio drops drastically. The peaks—fluorescent signals indicating specific successive nucleotides—become blurred and overlapping, making accurate base calling incredibly difficult.

* **The Challenge:** Standard separation of Gaussian distributions failed to accurately distinguish the true signal from background noise in these late-stage reads.
* **The Approach:** To increase the read accuracy of these genotype fragments, my team and I applied a wide array of classical machine learning techniques. We aggressively experimented with every available method, ultimately combining multiple techniques to filter noise and rescue reads where traditional algorithms failed.

### Exploring Deep Learning: Convolutional Neural Networks (CNNs)

As a second major initiative on the Sanger platform, I aimed to bypass classical feature engineering by applying Deep Neural Networks directly to the raw detector data.

* **The Approach:** I developed a unique, domain-specific Convolutional Neural Network (CNN) framework tailored entirely for analyzing Sanger sequencing chromatograms.
* **The Result:** While the architecture was innovative for this niche, the practical results were not groundbreaking. The CNN did not yield spectacular improvements in base-calling accuracy, highlighting the stubborn physical limits and extreme noise present in the raw data at the end of Sanger reads.

## Whole Genome Sequencing (WGS)

Whole Genome Sequencing (WGS) is a comprehensive method used to determine the entire DNA sequence of an organism's genome in a single process. Unlike Sanger sequencing, which targets specific fragments, WGS generates massive amounts of data by sequencing millions of DNA fragments simultaneously, which are then computationally mapped and reassembled.

### Algorithmic Optimization and Hardware Acceleration

Given the sheer volume of data produced by WGS, computational efficiency is a massive bottleneck. My work in this area shifted from signal processing to pure algorithmic optimization and high-performance computing.

* **The Approach:** I focused on optimizing existing, standard WGS algorithms. The core of this work involved analyzing the pipelines to identify parallelizable segments and rewriting them to execute concurrently wherever possible.
* **CPU Matrix Operations:** Furthermore, I optimized the code to heavily utilize CPU matrix operations, maximizing hardware efficiency.
* **The Result:** This project was a major success. The optimizations delivered spectacular performance gains, drastically reducing execution times and significantly accelerating the standard WGS data analysis algorithms.