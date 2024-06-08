import React from 'react';
import classnames from 'classnames';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import useBaseUrl from '@docusaurus/useBaseUrl';
import styles from './styles.module.css';

const features = [
  {
    title: <>Built with the Latest .NET Features</>,
    imageUrl: 'img/dot-net-core.svg',
    description: (
      <>
        Leveraging the newest capabilities of the latest .NET version, the .NET ActiveMQ Artemis Client ensures top performance and cutting-edge functionality.
      </>
    ),
  },
  {
    title: <>Asynchronous</>,
    imageUrl: 'img/undraw_synchronize_ccxk.svg',
    description: (
      <>
        .NET ActiveMQ Artemis Core Client was designed from the ground up to be fully asynchronous.
      </>
    ),
  },
  {
    title: <>High Performance</>,
    imageUrl: 'img/undraw_To_the_stars_qhyy.svg',
    description: (
      <>
        Utilizes the core protocol of Apache ActiveMQ Artemis to provide optimized speed and efficiency in processing messages.
      </>
    ),
  },
];

function Feature({ imageUrl, title, description }) {
  const imgUrl = useBaseUrl(imageUrl);
  return (
    <div className={classnames('col col--4', styles.feature)}>
      {imgUrl && (
        <div className="text--center">
          <img className={styles.featureImage} src={imgUrl} alt={title} />
        </div>
      )}
      <h3>{title}</h3>
      <p>{description}</p>
    </div>
  );
}

function Home() {
  const context = useDocusaurusContext();
  const { siteConfig = {} } = context;
  return (
    <Layout
      title={siteConfig.title}
      description="Unofficial ActiveMQ Artemis .NET CORE Client">
      <header className={classnames('hero hero--primary', styles.heroBanner)}>
        <div className="container">
          <h1 className="hero__title">{siteConfig.title}</h1>
          <p className="hero__subtitle">{siteConfig.tagline}</p>
          <div className={styles.buttons}>
            <Link
              className={classnames(
                'button button--outline button--secondary button--lg',
                styles.getStarted,
              )}
              to={useBaseUrl('docs/getting-started')}>
              Get Started
            </Link>
          </div>
        </div>
      </header>
      <main>
        {features && features.length && (
          <section className={styles.features}>
            <div className="container">
              <div className="row">
                {features.map((props, idx) => (
                  <Feature key={idx} {...props} />
                ))}
              </div>
            </div>
          </section>
        )}
      </main>
    </Layout>
  );
}

export default Home;
